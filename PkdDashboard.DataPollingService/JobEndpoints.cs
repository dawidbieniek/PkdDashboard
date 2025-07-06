using Hangfire;

using PkdDashboard.DataPollingService.Jobs;

namespace PkdDashboard.DataPollingService;

internal static class JobEndpoints
{
    public static void MapJobEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/job", (IBackgroundJobClient bgw, IQueryCompanyCountsJob job) =>
        {
            bgw.Enqueue(() => job.Execute());
        });
    }
}
