// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using WinGetStudio.Contracts.Services;

namespace WinGetStudio.Services.Settings;

public class AppSettingsService : IAppSettingsService
{
    private readonly ILogger<AppSettingsService> _logger;
    private readonly Dictionary<Type, IFeatureSettingsService> _featureSettings;

    public AppSettingsService(ILogger<AppSettingsService> logger, IEnumerable<IFeatureSettingsService> featureSettings)
    {
        _logger = logger;
        _featureSettings = featureSettings.ToDictionary(f => f.GetType());
    }

    /// <inheritdoc/>
    public async Task ApplySettingsAsync()
    {
        _logger.LogInformation("Applying application settings...");
        foreach (var feature in _featureSettings)
        {
            _logger.LogInformation($"Applying settings for feature: {feature.Key.Name}");
            await feature.Value.ApplySettingsAsync();
        }
    }

    /// <inheritdoc/>
    public TFeatureSettings GetFeature<TFeatureSettings>()
        where TFeatureSettings : IFeatureSettingsService
    {
        Debug.Assert(_featureSettings.ContainsKey(typeof(TFeatureSettings)), $"Feature settings of type {typeof(TFeatureSettings)} not found.");
        return (TFeatureSettings)_featureSettings[typeof(TFeatureSettings)];
    }
}
