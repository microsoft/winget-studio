// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Management.Configuration;
using WinGetStudio.Exceptions;
using WinGetStudio.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Extensions;
using WinGetStudio.Services.DesiredStateConfiguration.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Models.Schemas.ConfigurationV3;

namespace WinGetStudio.ViewModels;

public delegate UnitViewModel UnitViewModelFactory();

public partial class UnitViewModel : ObservableObject
{
    private readonly IStringLocalizer<UnitViewModel> _localizer;
    private readonly ILogger<UnitViewModel> _logger;
    private readonly IDSC _dsc;
    private readonly UnitViewModelFactory _unitFactory;

    public IDSCUnit? Unit { get; set; }

    public string IdOrDefault => string.IsNullOrWhiteSpace(Id) ? DefaultId : Id;

    public string DefaultId { get; } = Guid.NewGuid().ToString();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanShowDetails))]
    public partial UnitDetailsViewModel? Details { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanShowDetails))]
    public partial bool AreDetailsLoading { get; set; }

    public bool CanShowDetails => Details != null || AreDetailsLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IdOrDefault))]
    public partial string? Id { get; set; }

    [ObservableProperty]
    public partial Guid? InstanceId { get; set; }

    [ObservableProperty]
    public partial string? Title { get; set; }

    [ObservableProperty]
    public partial string? Description { get; set; }

    [ObservableProperty]
    public partial string? Intent { get; set; }

    [ObservableProperty]
    public partial List<UnitViewModel>? Dependencies { get; set; }

    [ObservableProperty]
    public partial DSCPropertySet? Settings { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MetadataList))]
    public partial DSCPropertySet? Metadata { get; set; }

    [ObservableProperty]
    public partial string? SettingsText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiresElevation))]
    public partial UnitSecurityContext? SelectedSecurityContext { get; set; }

    public bool RequiresElevation => SelectedSecurityContext?.SecurityContext == SecurityContext.Elevated;

    public IList<KeyValuePair<string, object>>? MetadataList => Metadata?.ToList();

    public UnitViewModel(
        IStringLocalizer<UnitViewModel> localizer,
        ILogger<UnitViewModel> logger,
        IDSC dsc,
        UnitViewModelFactory unitFactory)
    {
        _localizer = localizer;
        _logger = logger;
        _dsc = dsc;
        _unitFactory = unitFactory;
        SelectedSecurityContext = UnitSecurityContext.Default;
        Dependencies = [];
        Settings = [];
        Metadata = [];
    }

    private string GetTitle(IDSCUnit unit)
    {
        return unit.ModuleName == string.Empty ? unit.Type : $"{unit.ModuleName}/{unit.Type}";
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            throw new DSCUnitValidationException(_localizer["Unit_TitleCannotBeNullOrEmpty"]);
        }

        if (!string.IsNullOrEmpty(SettingsText))
        {
            DSCPropertySet.FromYaml(SettingsText);
        }
    }

    /// <summary>
    /// Creates a configuration object representing this DSC unit.
    /// </summary>
    /// <returns>The configuration object.</returns>
    public ConfigurationV3 ToConfigurationV3()
    {
        Validate();
        Debug.Assert(!string.IsNullOrEmpty(Title), "Title should not be null or empty after validation.");
        var dependencies = Dependencies?.Select(d => d.IdOrDefault).ToList();
        var dependencyNames = dependencies?.Count == 0 ? null : dependencies;
        var properties = Settings?.Count == 0 ? null : Settings?.DeepCopy();
        var metadata = Metadata?.Count == 0 ? null : Metadata?.DeepCopy();
        var additionalProperties = metadata == null ? [] : new Dictionary<string, object> { { "metadata", metadata } };
        var config = new ConfigurationV3()
        {
            Resources =
            [
                new()
                {
                    Name = IdOrDefault,
                    Type = Title,
                    DependsOn = dependencyNames,
                    Properties = properties,
                    AdditionalProperties = additionalProperties,
                }
            ],
        };

        // Add WinGet metadata to the configuration.
        config.AddWinGetMetadata();

        // Add resource metadata
        if (Metadata != null && Metadata.Count > 0)
        {
            config.Resources[0].AddMetadata(Metadata.DeepCopy());
        }

        // Add security context metadata if specified and not default.
        if (SelectedSecurityContext != null && SelectedSecurityContext != UnitSecurityContext.Default)
        {
            config.Resources[0].AddSecurityContext(SelectedSecurityContext.Value);
        }

        // Add description if specified.
        if (!string.IsNullOrWhiteSpace(Description))
        {
            config.Resources[0].AddDescription(Description);
        }

        return config;
    }

    /// <summary>
    /// Copies the properties from another instance.
    /// </summary>
    /// <param name="source">The source instance.</param>
    public async Task CopyFromAsync(UnitViewModel source)
    {
        Unit = source.Unit;
        Id = source.Id;
        InstanceId = source.InstanceId;
        Title = source.Title;
        Description = source.Description;
        SelectedSecurityContext = source.SelectedSecurityContext;
        Intent = source.Intent;
        Dependencies = source.Dependencies?.ToList();
        SettingsText = source.SettingsText;
        Details = source.Details;

        // Re-parse the settings text to ensure we have an up-to-date object.
        (Metadata, Settings) = await Task.Run(() =>
        {
            var m = source.Metadata?.DeepCopy();
            var s = string.IsNullOrEmpty(SettingsText) ? null : DSCPropertySet.FromYaml(SettingsText);
            return (m, s);
        });
    }

    public async Task CopyFromAsync(IDSCUnit unit)
    {
        Unit = unit;
        Id = unit.Id;
        InstanceId = unit.InstanceId;
        Title = GetTitle(unit);
        Description = unit.Description;
        SelectedSecurityContext = UnitSecurityContext.FromEnum(unit.SecurityContext);
        Intent = unit.Intent;
        Details = unit.Details == null ? null : new(unit.Details);
        Dependencies = [..unit.Dependencies.Select(id =>
        {
            var unit = _unitFactory();
            unit.Id = id;
            return unit;
        })];
        (Settings, SettingsText, Metadata) = await Task.Run(() =>
        {
            var m = unit.Metadata.DeepCopy();
            var s = unit.Settings.DeepCopy();
            var st = s.ToYaml();
            return (s, st, m);
        });
    }

    public void ResolveDependencies(SetViewModel dscSet)
    {
        // If there are no dependencies, nothing to resolve.
        if (Dependencies == null || Dependencies.Count == 0)
        {
            return;
        }

        // Replace each dependency with the matching available unit, if found.
        var resolvedDependencies = new List<UnitViewModel>();
        foreach (var dep in Dependencies)
        {
            var match = dscSet.Units.FirstOrDefault(u => dep.IdOrDefault == u.IdOrDefault);
            if (match != null)
            {
                resolvedDependencies.Add(match);
            }
        }

        // Update the dependencies list.
        Dependencies = resolvedDependencies;
    }

    /// <summary>
    /// Creates a clone of this instance.
    /// </summary>
    /// <returns>A copy of this instance.</returns>
    public async Task<UnitViewModel> CloneAsync()
    {
        var clone = _unitFactory();
        await clone.CopyFromAsync(this);
        return clone;
    }

    [RelayCommand]
    private async Task OnExpandingAsync()
    {
        await LoadDetailsAsync();
    }

    /// <summary>
    /// Loads the unit details asynchronously.
    /// </summary>
    private async Task LoadDetailsAsync()
    {
        if (!AreDetailsLoading && Details == null && Unit != null)
        {
            AreDetailsLoading = true;
            try
            {
                var result = await _dsc.GetUnitDetailsAsync(Unit);
                if (result.UnitDetails != null && (result.ResultInformation?.IsOk ?? true))
                {
                    _logger.LogInformation($"Loaded unit details for unit {IdOrDefault}");
                    Details = new UnitDetailsViewModel(result.UnitDetails);
                }
                else
                {
                    _logger.LogError(result.ResultInformation?.ResultCode, $"Failed to load unit details for unit {IdOrDefault}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while loading unit details for unit {IdOrDefault}");
            }
            finally
            {
                AreDetailsLoading = false;
            }
        }
    }
}
