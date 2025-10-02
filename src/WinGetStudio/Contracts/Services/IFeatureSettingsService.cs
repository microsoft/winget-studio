// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.Contracts.Services;

public interface IFeatureSettingsService
{
    /// <summary>
    /// Applies the current settings.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ApplySettingsAsync();
}
