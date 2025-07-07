namespace PkdDashboard.DataPollingService.Jobs;

internal interface IQueryCompanyCountsJob
{
    public static string JobId => nameof(IQueryCompanyCountsJob);

    Task ExecuteAsync(CancellationToken cancellationToken);
}
