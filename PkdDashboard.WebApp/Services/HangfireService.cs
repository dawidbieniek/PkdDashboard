using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;

using PkdDashboard.WebApp.Jobs;

using System.Linq.Expressions;

namespace PkdDashboard.WebApp.Services;

internal class HangfireService(IBackgroundJobClient bgw, IRecurringJobManager jobManager)
{
    private readonly IBackgroundJobClient _bgw = bgw;
    private readonly IRecurringJobManager _jobManager = jobManager;

    public static bool IsJobScheduled<T>() where T : IJob
    {
        var monitor = JobStorage.Current.GetConnection();
        var recurringJobs = monitor.GetRecurringJobs();

        return recurringJobs.Any(x => x.Id == IJobUtil.GetJobId<T>());
    }

    public void EnableJobPeriodicRun<T>(Expression<Func<T, Task>> jobExecute, int utcHour) where T : IJob
    {
        _jobManager.AddOrUpdate(IJobUtil.GetJobId<T>(),
            Job.FromExpression(jobExecute),
            Cron.Daily(utcHour),
            new RecurringJobOptions()
            {
                TimeZone = TimeZoneInfo.Local
            });
    }

    public void DisableJobPeriodicRun<T>() where T : IJob
    {
        _jobManager.RemoveIfExists(IJobUtil.GetJobId<T>());
    }

    public void RunJob<T>(Expression<Func<T, Task>> jobExecute) where T : IJob
    {
        _bgw.Enqueue(jobExecute);
    }
}