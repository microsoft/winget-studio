// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Helpers;

namespace WinGetStudio.Services;

internal class AppInfoService : IAppInfoService
{
        public string GetAppNameLocalized()
    {
#if STABLE_BUILD
        return "AppDisplayNameStable".GetLocalized();
#else
        return "AppDisplayNameDev".GetLocalized();
#endif
    }

}
