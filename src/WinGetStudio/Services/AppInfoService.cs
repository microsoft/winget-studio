// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Contracts.Services;
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
}
