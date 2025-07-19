using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;

using PkdDashboard.WebApp.Jobs;

namespace PkdDashboard.WebApp.Services;

internal class HangfireService(IBackgroundJobClient bgw, IRecurringJobManager jobManager)
{
    private readonly IBackgroundJobClient _bgw = bgw;
    private readonly IRecurringJobManager _jobManager = jobManager;

    public static bool IsJobScheduled()
    {
        var monitor = JobStorage.Current.GetConnection();
        var recurringJobs = monitor.GetRecurringJobs();

        return recurringJobs.Any(x => x.Id == IQueryCompanyCountsJob.JobId);
    }

    public void EnableJobPeriodicRun()
    {
        _jobManager.AddOrUpdate(IQueryCompanyCountsJob.JobId,
            Job.FromExpression<IQueryCompanyCountsJob>(j => j.ExecuteAsync(CancellationToken.None)),
            Cron.Daily(8),  // Run the job at 10 ECT
            new RecurringJobOptions()
            {
                TimeZone = TimeZoneInfo.Local
            });
    }

    public void DisableJobPeriodicRun()
    {
        _jobManager.RemoveIfExists(IQueryCompanyCountsJob.JobId);
    }

    public void RunJob()
    {
        _bgw.Enqueue<IQueryCompanyCountsJob>(job => job.ExecuteAsync(CancellationToken.None));
    }
}