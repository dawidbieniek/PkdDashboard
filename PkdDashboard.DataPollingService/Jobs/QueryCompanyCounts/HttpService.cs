using System.Text.Json;

namespace PkdDashboard.DataPollingService.Jobs.QueryCompanyCounts;

internal class HttpService(ILogger<HttpService> logger, IHttpClientFactory httpClientFactory)
{
    private const string StatusQuery = "status=AKTYWNY&status=OCZEKUJE_NA_ROZPOCZECIE_DZIALANOSCI&status=WYLACZNIE_W_FORMIE_SPOLKI";
    private const string LimitQuery = "limit=1";
    private const string CountPropertyName = "count";

    private const string RateLimitHeaderName = "X-Rate-Limit-Remaining";

    private const int MinJitterDelayMs = 200;
    private const int MaxJitterDelayMs = 800;

    private readonly ILogger<HttpService> _logger = logger;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    private async Task<(int? Count, int? RemainingRateRequests)> GetNumberOfCompaniesInPkdAsync(string pkdQuery, CancellationToken cancellationToken)
    {
        var http = _httpClientFactory.CreateClient(HttpClientKeys.BiznesGovKey);
        var response = await http.GetAsync($"firmy?pkd={pkdQuery}&{StatusQuery}&{LimitQuery}", cancellationToken);

        int? remainingRateRequests = null;
        if (response.Headers.TryGetValues(RateLimitHeaderName, out var values))
        {
            var headerValue = values.FirstOrDefault();
            if (int.TryParse(headerValue, out int parsed))
                remainingRateRequests = parsed;
        }

        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            _logger.LogError("Too many requests to the API.");
            return (null, remainingRateRequests);
        }
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Recieved error status code: {code}", response.StatusCode);
            return (null, remainingRateRequests);
        }
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            return (0, remainingRateRequests);

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (json.RootElement.TryGetProperty(CountPropertyName, out var countElement) && countElement.ValueKind == JsonValueKind.Number)
        {
            int count = countElement.GetInt32();
            return (count, remainingRateRequests);
        }

        _logger.LogError("Count property is missing or not a number.");
        return (null, remainingRateRequests);
    }

    public async Task<IEnumerable<int?>> RequestBatchOfNumberOfCompaniesInPkdAsync(List<string> pkdQueries, CancellationToken cancellationToken)
    {
        List<int?> results = new(pkdQueries.Count);
        int? lastRateLimit = null;

        foreach (var pkdQuery in pkdQueries)
        {
            (int? count, int? rateLimit) = await GetNumberOfCompaniesInPkdAsync(pkdQuery, cancellationToken);
            results.Add(count);

            if (rateLimit.HasValue)
                lastRateLimit = rateLimit;

            await JitterDelay(cancellationToken);
        }

        if (lastRateLimit.HasValue)
            _logger.LogInformation("{rateLimitName} remaining after batch: {rateLimit}", RateLimitHeaderName, lastRateLimit);

        return results;
    }

    private static Task JitterDelay(CancellationToken cancellationToken)
    {
        int delay = Random.Shared.Next(MinJitterDelayMs, MaxJitterDelayMs + 1);
        return Task.Delay(delay, cancellationToken);
    }
}