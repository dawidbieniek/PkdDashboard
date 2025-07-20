namespace PkdDashboard.WebApp.Jobs.QueryCompanyCounts.Exceptions;

[Serializable]
public class HttpApiErrorException : Exception
{
    public string? LastUpdatedPkd { get; private init; }

    public HttpApiErrorException()
    { }

    public HttpApiErrorException(string? lastUpdatedPkd)
    {
        LastUpdatedPkd = lastUpdatedPkd;
    }

    public HttpApiErrorException(string? lastUpdatedPkd, string message) : base(message)
    {
        LastUpdatedPkd = lastUpdatedPkd;
    }

    public HttpApiErrorException(string? lastUpdatedPkd, string message, Exception inner) : base(message, inner)
    {
        LastUpdatedPkd = lastUpdatedPkd;
    }
}