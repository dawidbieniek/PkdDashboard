using PkdDashboard.Shared.DateUtil;

namespace PkdDashboard.DataPollingService.Jobs.QueryCompanyCounts;

internal class QueryCompanyCountsJob(ILogger<QueryCompanyCountsJob> logger, DatabaseService databaseService, HttpService httpService) : IQueryCompanyCountsJob
{
    private const int BatchSize = 20;
    private const int BatchDelayMs = 4000;

    private readonly ILogger<QueryCompanyCountsJob> _logger = logger;
    private readonly DatabaseService _databaseService = databaseService;
    private readonly HttpService _httpService = httpService;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        DateOnly today = DateUtil.TodayDate;
        _logger.LogInformation("Starting company count query for {date}", today);

        if (_databaseService.IsTodayEntryPresent(today))
            _logger.LogWarning("Today's company count entry already exists.");

        var pkdsForQuery = _databaseService.GetPkdsForQuery();

        if (pkdsForQuery.Count > 1000)
        {
            _logger.LogWarning("The number of PKDs to query is greater than 1000. Truncating the request list");
            pkdsForQuery = [.. pkdsForQuery.Take(1000)];
        }

        _logger.LogInformation("Querying company counts for {count} PKDs in {batchCount} batches of size {batchSize}", pkdsForQuery.Count, (pkdsForQuery.Count / BatchSize) + 1, BatchSize);

        for (int i = 0; i < pkdsForQuery.Count; i += BatchSize)
        {
            var batch = pkdsForQuery.Skip(i).Take(BatchSize).ToList();
            _logger.LogInformation("Processing batch {batchIndex} with {batchSize} PKDs", i / BatchSize + 1, batch.Count);

            var counts = (await _httpService.RequestBatchOfNumberOfCompaniesInPkdAsync([.. batch.Select(pkd => pkd.ToQueryString())], cancellationToken)).ToList();

            for (int j = i; j < i + batch.Count && j < pkdsForQuery.Count; j++)
            {
                var pkd = pkdsForQuery[j];
                int? count = counts[j - i];
                if (count is null)
                {
                    _logger.LogError("Failed to retrieve company count for PKD: {pkd}", pkd.ToQueryString());
                    continue;
                }
                _logger.LogDebug("PKD: {pkd}, Count: {count}", pkd.ToQueryString(), count);
                await _databaseService.SaveCompanyCountAsync(today, pkd, count.Value, cancellationToken);
            }

            _logger.LogInformation("Batch {batchIndex} processed. Sleeping for {delay} ms", i / BatchSize + 1, BatchDelayMs);
            await Task.Delay(BatchDelayMs, cancellationToken);
        }

        _logger.LogInformation("Company count query completed for {date}", today);
    }
}