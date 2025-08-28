// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Windows.ApplicationModel;
using Windows.Storage;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Extensions;
using WinGetStudio.Helpers;

namespace WinGetStudio.Services;

internal sealed class AppInfoService : IAppInfoService
{
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
    public string GetAppLogsFolder()
    {
        return Path.Combine(ApplicationData.Current.TemporaryFolder.Path, "Logs");
    }
}
