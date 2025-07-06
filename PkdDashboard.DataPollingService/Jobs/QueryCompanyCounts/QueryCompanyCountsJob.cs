namespace PkdDashboard.DataPollingService.Jobs.QueryCompanyCounts;

internal class QueryCompanyCountsJob(ILogger<QueryCompanyCountsJob> logger, DatabaseService databaseService, HttpService httpService) : IQueryCompanyCountsJob
{
    private readonly ILogger<QueryCompanyCountsJob> _logger = logger;
    private readonly DatabaseService _databaseService = databaseService;
    private readonly HttpService _httpService = httpService;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        DateTime today = DateTime.UtcNow.AddHours(2).Date; // We want local time in Poland (UTC+2)
        _logger.LogInformation("Starting company count query for {date}", today);

        if (_databaseService.IsTodayEntryPresent(today))
            _logger.LogWarning("Today's company count entry already exists.");

        var pkdsForQuery = _databaseService.GetPkdsForQuery();

        if (pkdsForQuery.Count > 1000)
        {
            _logger.LogWarning("The number of PKDs to query is greater than 1000. Truncating the request list");
            pkdsForQuery = [.. pkdsForQuery.Take(1000)];
        }

        _logger.LogInformation("Querying company counts for {count} PKDs", pkdsForQuery.Count);

        foreach (var pkd in pkdsForQuery)
        {
            int? count = await _httpService.GetNumberOfCompaniesInPkdAsync(pkd.ToQueryString(), cancellationToken);
            if (count is null)
                return;

            _logger.LogDebug("PKD: {pkd}, Count: {count}", pkd.ToQueryString(), count);

            await _databaseService.SaveCompanyCountAsync(today, pkd, count.Value, cancellationToken);
        }

        _logger.LogInformation("Company count query completed for {date}", today);
    }
}