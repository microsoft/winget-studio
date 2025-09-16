// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Contracts.Services;
using WinGetStudio.Extensions;
using WinGetStudio.Services.Core.Helpers;

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
        var version = RuntimeHelper.GetAppVersion();
        return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    /// <inheritdoc/>
    public string GetAppLogsPath()
    {
        return RuntimeHelper.GetAppLogsPath();
    }

    /// <inheritdoc/>
    public string GetAppInstanceLogPath()
    {
        return RuntimeHelper.GetAppInstanceLogPath();
    }
}
