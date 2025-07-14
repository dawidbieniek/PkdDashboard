namespace PkdDashboard.Global;

public static class ServiceDiscoveryUtil
{
    public static string GetServiceEndpoint(string serviceName, string endpointName = "https", int index = 0) =>
         Environment.GetEnvironmentVariable($"services__{serviceName}__{endpointName}__{index}")
        ?? throw new InvalidOperationException($"'{serviceName}' service with endpoint '{endpointName}' not found");
}