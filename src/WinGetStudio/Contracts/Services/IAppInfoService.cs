// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.Services;

public interface IAppInfoService
{
    /// <summary>
    /// Gets the localized application name.
    /// </summary>
    public string GetAppNameLocalized();
}
