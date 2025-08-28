// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Reflection;
using Windows.ApplicationModel;
using Windows.Storage;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Extensions;
using WinGetStudio.Helpers;

namespace WinGetStudio.Services;

internal sealed class AppInfoService : IAppInfoService
{
    // Unique log path for each app instance
    private readonly string _instanceLogPath = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff", CultureInfo.InvariantCulture);

    /// <inheritdoc/>
    public string GetAppNameLocalized()
    {
#if STABLE_BUILD
        return "AppDisplayNameStable".GetLocalized();
#else
        return "AppDisplayNameDev".GetLocalized();
#endif
    }

    /// <inheritdoc/>
    public string GetAppVersion()
    {
        Version version;
        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;
            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    /// <inheritdoc/>
    public string GetAppLogsPath()
    {
        return Path.Combine(ApplicationData.Current.TemporaryFolder.Path, "Logs");
    }

    /// <inheritdoc/>
    public string GetAppInstanceLogPath()
    {
        return Path.Combine(GetAppLogsPath(), _instanceLogPath);
    }
}
