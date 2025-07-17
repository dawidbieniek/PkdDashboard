using System.Globalization;

namespace PkdDashboard.Shared.Configuration;

public static class LocalizationUtil
{
    private const string DefaultCulture = "pl-PL";

    public static void SetDefaultCulture()
    {
        var culture = new CultureInfo(DefaultCulture);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}