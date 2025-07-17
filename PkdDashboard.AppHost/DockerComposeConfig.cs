using Aspire.Hosting.Docker.Resources.ComposeNodes;

namespace PkdDashboard.AppHost;

internal static class DockerComposeConfig
{
    public const string ComposeEnvironmentName = "docker-compose";

    public static class Networks
    {
        public const string ProxyNetKey = "proxy-net";
        public const string PkdNetKey = "pkd-net";

        public static readonly Network ProxyNet = new()
        {
            Name = ProxyNetKey,
            External = true
        };

        public static readonly Network PkdNet = new()
        {
            Name = PkdNetKey,
            External = false,
            Driver = "bridge"
        };
    }

    public static class Volumes
    {
        public const string DatabaseVolumeKey = "pgdata";
    }
}