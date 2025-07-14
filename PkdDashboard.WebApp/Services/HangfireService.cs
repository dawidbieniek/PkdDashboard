namespace PkdDashboard.WebApp.Services;

internal class HangfireService(ILogger<HangfireService> logger, IHttpClientFactory httpClientFactory)
{
    private readonly ILogger<HangfireService> _logger = logger;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<bool> IsJobScheduledAsync()
    {
        var http = _httpClientFactory.CreateClient(HttpClientKeys.HangfireClientKey);
        var response = await http.GetAsync("/job/status");


        return bool.Parse(await response.Content.ReadAsStringAsync());
    }

    public async Task ScheduleJobAsync()
    {
        var http = _httpClientFactory.CreateClient(HttpClientKeys.HangfireClientKey);
        var response = await http.GetAsync("/job/start");

        ;
    }

    public async Task UnScheduleJobAsync()
    {
        var http = _httpClientFactory.CreateClient(HttpClientKeys.HangfireClientKey);
        var response = await http.GetAsync("/job/stop");

        ;
    }


    public async Task TriggerJobAsync()
    {
        var http = _httpClientFactory.CreateClient(HttpClientKeys.HangfireClientKey);
        var response = await http.GetAsync("/job/force");

        ;
    }
}
