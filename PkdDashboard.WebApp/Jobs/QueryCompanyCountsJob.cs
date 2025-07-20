using PkdDashboard.Shared.DateUtil;
using PkdDashboard.WebApp.Data.Entities;
using PkdDashboard.WebApp.Jobs.QueryCompanyCounts;
using PkdDashboard.WebApp.Jobs.QueryCompanyCounts.Exceptions;

namespace PkdDashboard.WebApp.Jobs;

internal class QueryCompanyCountsJob(ILogger<QueryCompanyCountsJob> logger, DatabaseService databaseService, HttpService httpService) : IJob
{
    private const int BatchSize = 10;
    private const int BatchDelayMs = 1000;

    private readonly ILogger<QueryCompanyCountsJob> _logger = logger;
    private readonly DatabaseService _databaseService = databaseService;
    private readonly HttpService _httpService = httpService;

    public static string JobId => nameof(QueryCompanyCountsJob);

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        DateOnly today = DateUtil.TodayDate;
        _logger.LogInformation("Starting company count query for {date}", today);

        if (_databaseService.IsTodayEntryPresent(today))
            _logger.LogWarning("Today's company count entry already exists.");

        var pkdsForQuery = _databaseService.GetPkdsForQuery();

        if (pkdsForQuery.Count > 1000)
        {
            _logger.LogWarning("The number of PKDs to query is greater than 900. Truncating the request list");
            pkdsForQuery = [.. pkdsForQuery.Take(900)];
        }

        var numberOfBatches = (pkdsForQuery.Count / BatchSize) + 1;
        _logger.LogInformation("Querying company counts for {count} PKDs in {batchCount} batches of size {batchSize}", pkdsForQuery.Count, numberOfBatches, BatchSize);

        PkdEntry? lastUpdatedPkd = null;
        try
        {
            for (int i = 0; i < pkdsForQuery.Count; i += BatchSize)
            {
                var batch = pkdsForQuery.Skip(i).Take(BatchSize).ToList();
                _logger.LogInformation("Processing batch {batchIndex}/{batchCount} with {batchSize} PKDs", (i / BatchSize) + 1, numberOfBatches, batch.Count);

                var counts = (await _httpService.RequestBatchOfNumberOfCompaniesInPkdAsync([.. batch.Select(pkd => pkd.ToQueryString())], cancellationToken)).ToList();

                for (int j = i; j < i + batch.Count && j < pkdsForQuery.Count; j++)
                {
                    var pkd = pkdsForQuery[j];
                    int count = counts[j - i];
                    _logger.LogDebug("PKD: {pkd}, Count: {count}", pkd.ToQueryString(), count);
                    await _databaseService.SaveCompanyCountAsync(today, pkd, count, cancellationToken);
                    lastUpdatedPkd = pkd;
                }

                _logger.LogInformation("Batch {batchIndex}/{batchCount} processed. Sleeping for {delay} ms", (i / BatchSize) + 1, numberOfBatches, BatchDelayMs);
                await Task.Delay(BatchDelayMs, cancellationToken);
            }

            _logger.LogInformation("Company count query completed for {date}", today);
        }
        catch (HttpApiErrorException ex)
        {
            _logger.LogCritical("Failed to fetch all counts: '{message}. Last updated pkd: {lastUpdated}'", ex.Message, lastUpdatedPkd?.PkdString ?? "???");
            // TODO: Save last correct id and queue retry in at least an hour
            return;
        }
    }
}