using System.Text.Json;

namespace PkdDashboard.DataPollingService.Jobs.QueryCompanyCounts;

internal class HttpService(ILogger<HttpService> logger, IHttpClientFactory httpClientFactory)
{
    private const string StatusQuery = "status=AKTYWNY&status=OCZEKUJE_NA_ROZPOCZECIE_DZIALANOSCI&status=WYLACZNIE_W_FORMIE_SPOLKI";
    private const string LimitQuery = "limit=1";
    private const string CountPropertyName = "count";

    private readonly ILogger<HttpService> _logger = logger;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<int?> GetNumberOfCompaniesInPkdAsync(string pkdQuery, CancellationToken cancellationToken)
    {
        var http = _httpClientFactory.CreateClient(HttpClientKeys.BiznesGovKey);
        var response = await http.GetAsync($"firmy?pkd={pkdQuery}&{StatusQuery}&{LimitQuery}", cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            _logger.LogError("Too many requests to the API.");
            return null;
        }
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Recieved error status code: {code}", response.StatusCode);
            return null;
        }
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            return 0;

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (json.RootElement.TryGetProperty(CountPropertyName, out var countElement) && countElement.ValueKind == JsonValueKind.Number)
        {
            int count = countElement.GetInt32();
            return count;
        }

        _logger.LogError("Count property is missing or not a number.");
        return null;
    }
}