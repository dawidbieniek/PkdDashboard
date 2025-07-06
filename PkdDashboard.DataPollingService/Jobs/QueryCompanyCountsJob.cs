namespace PkdDashboard.DataPollingService.Jobs;

internal class QueryCompanyCountsJob(ILogger<QueryCompanyCountsJob> logger) : IQueryCompanyCountsJob
{
    private readonly ILogger<QueryCompanyCountsJob> _logger = logger;

    public void Execute()
    {
        _logger.LogWarning("Executing the job");
    }
}
