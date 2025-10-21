// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Localization;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Services.Core.Helpers;

namespace WinGetStudio.Services;

internal sealed class AppInfoService : IAppInfoService
{
    private readonly IStringLocalizer<AppInfoService> _localizer;

    public AppInfoService(IStringLocalizer<AppInfoService> localizer)
    {
        _localizer = localizer;
    }

    /// <inheritdoc/>
    public string GetAppNameLocalized()
    {
#if STABLE_BUILD
        return _localizer["AppDisplayNameStable"];
#else
        return _localizer["AppDisplayNameDev"];
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
