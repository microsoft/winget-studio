// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using WinGetStudio.Contracts.Services;

namespace WinGetStudio.Services;

public class AppSettings : IAppSettingsService
{
    private readonly ILogger<AppSettings> _logger;
    private readonly Dictionary<Type, IFeatureSettingsService> _featureSettings;

    public AppSettings(ILogger<AppSettings> logger, IEnumerable<IFeatureSettingsService> featureSettings)
    {
        _logger = logger;
        _featureSettings = featureSettings.ToDictionary(f => f.GetType());
    }

    public async Task ApplySettingsAsync()
    {
        _logger.LogInformation("Applying application settings...");
        foreach (var feature in _featureSettings)
        {
            _logger.LogInformation($"Applying settings for feature: {feature.Key.Name}");
            await feature.Value.ApplySettingsAsync();
        }
    }

    public TFeatureSettings GetFeature<TFeatureSettings>()
        where TFeatureSettings : IFeatureSettingsService
    {
        Debug.Assert(_featureSettings.ContainsKey(typeof(TFeatureSettings)), $"Feature settings of type {typeof(TFeatureSettings)} not found.");
        return (TFeatureSettings)_featureSettings[typeof(TFeatureSettings)];
    }
}
