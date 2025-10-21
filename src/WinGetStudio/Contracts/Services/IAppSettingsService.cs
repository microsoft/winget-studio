// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.Contracts.Services;

public interface IAppSettingsService
{
    /// <summary>
    /// Applies all settings from all feature settings services.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ApplySettingsAsync();

    /// <summary>
    /// Gets the feature settings service of the specified type.
    /// </summary>
    /// <typeparam name="TFeatureSettings">The type of the feature settings service.</typeparam>
    /// <returns>The feature settings service.</returns>
    TFeatureSettings GetFeature<TFeatureSettings>()
        where TFeatureSettings : IFeatureSettingsService;
}
