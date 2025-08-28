// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.Contracts.Services;

public interface IAppInfoService
{
    /// <summary>
    /// Gets the localized application name.
    /// </summary>
    /// <returns>The localized application name.</returns>
    public string GetAppNameLocalized();

    /// <summary>
    /// Gets the application version.
    /// </summary>
    /// <returns>The application version.</returns>
    public string GetAppVersion();

    /// <summary>
    /// Gets the application logs folder.
    /// </summary>
    /// <returns>The application logs folder.</returns>
    public string GetAppLogsFolder();
}
