// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Resources.Core;

namespace WingetStudio.Services.Localization.Services;

internal sealed class ReswStringLocalizer : IStringLocalizer
{
    private readonly ResourceLoader _loader;
    private readonly ResourceContext _context;
    private readonly ResourceMap _map;
    private readonly ILogger<ResourceLoader> _logger;

    public ReswStringLocalizer(ILogger<ResourceLoader> logger)
    {
        _loader = new ResourceLoader();
        _context = ResourceContext.GetForViewIndependentUse();
        _map = ResourceManager.Current.MainResourceMap.GetSubtree("Resources");
        _logger = logger;
    }

    /// <inheritdoc/>
    public LocalizedString this[string name] => Lookup(name, null);

    /// <inheritdoc/>
    public LocalizedString this[string name, params object[] arguments] => Lookup(name, arguments);

    /// <inheritdoc/>
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        foreach (var entry in _map)
        {
            var value = entry.Value.Resolve(_context).ValueAsString;
            if (value != null)
            {
                yield return new LocalizedString(entry.Key, value.ToString(), false);
            }
        }
    }

    /// <summary>
    /// Looks up a localized string by its name and formats it with the provided arguments if any.
    /// </summary>
    /// <param name="name">The name of the localized string to look up.</param>
    /// <param name="args">The arguments to format the localized string with.</param>
    /// <returns>>A LocalizedString containing the result of the lookup and formatting.</returns>
    private LocalizedString Lookup(string name, object[] args)
    {
        string value;
        bool notFound;
        try
        {
            value = _loader.GetString(name);
            notFound = string.IsNullOrEmpty(value);
            if (notFound)
            {
                value = name;
                _logger.LogWarning($"Localized string for key '{name}' not found. Using key as fallback.");
            }
            else if (args is { Length: > 0 })
            {
                value = string.Format(CultureInfo.CurrentCulture, value, args);
            }
        }
        catch
        {
            value = name;
            notFound = true;
            _logger.LogWarning($"Failed to load localized string for key '{name}'. Using key as fallback.");
        }

        return new LocalizedString(name, value, notFound);
    }
}
