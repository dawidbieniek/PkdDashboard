namespace PkdDashboard.WebApp.Jobs;

public interface IJob
{
    abstract static string JobId { get; }
}

public static class  IJobUtil
{
    public static string GetJobId<T>() where T : IJob
        => T.JobId;
}