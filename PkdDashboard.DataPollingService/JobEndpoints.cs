using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;

using PkdDashboard.DataPollingService.Jobs;

namespace PkdDashboard.DataPollingService;

internal static class JobEndpoints
{
    public static void MapJobEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/job/start", (IBackgroundJobClient bgw, IRecurringJobManager jobManager, CancellationToken cancellationToken) =>
        {
            jobManager.AddOrUpdate(IQueryCompanyCountsJob.JobId,
                Job.FromExpression<IQueryCompanyCountsJob>(j => j.ExecuteAsync(cancellationToken)),
                Cron.Daily(8),  // Run the job at 10 ECT
                new RecurringJobOptions()
                {
                    TimeZone = TimeZoneInfo.Local
                });
        });
        app.MapGet("/job/stop", (IRecurringJobManager jobManager) =>
        {
            jobManager.RemoveIfExists(IQueryCompanyCountsJob.JobId);
        });
        app.MapGet("/job/status", (IBackgroundJobClient bgw, CancellationToken cancellationToken) =>
        {
            var monitor = JobStorage.Current.GetConnection();
            var recurringJobs = monitor.GetRecurringJobs();

            return recurringJobs.Any(x => x.Id == IQueryCompanyCountsJob.JobId);
        });
        app.MapGet("/job/force", (IBackgroundJobClient bgw, IQueryCompanyCountsJob job, CancellationToken cancellationToken) =>
        {
            bgw.Enqueue(() => job.ExecuteAsync(cancellationToken));
        });
    }
}