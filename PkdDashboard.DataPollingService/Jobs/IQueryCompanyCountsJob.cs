namespace PkdDashboard.DataPollingService.Jobs;

internal interface IQueryCompanyCountsJob
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}
