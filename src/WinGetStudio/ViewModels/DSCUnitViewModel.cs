// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Management.Configuration;
using WinGetStudio.Exceptions;
using WinGetStudio.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Extensions;
using WinGetStudio.Services.DesiredStateConfiguration.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Models.Schemas.ConfigurationV3;

namespace WinGetStudio.ViewModels;

public partial class DSCUnitViewModel : ObservableObject
{
    public IDSCUnit? Unit { get; set; }

    public string IdOrDefault => string.IsNullOrWhiteSpace(Id) ? DefaultId : Id;

    public string DefaultId { get; } = Guid.NewGuid().ToString();

    [ObservableProperty]
    public partial DSCUnitDetailsViewModel? Details { get; set; }

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
    public partial List<DSCUnitViewModel>? Dependencies { get; set; }

    [ObservableProperty]
    public partial DSCPropertySet? Settings { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MetadataList))]
    public partial DSCPropertySet? Metadata { get; set; }

    [ObservableProperty]
    public partial string? SettingsJson { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiresElevation))]
    public partial UnitSecurityContext? SelectedSecurityContext { get; set; }

    public bool RequiresElevation => SelectedSecurityContext?.SecurityContext == SecurityContext.Elevated;

    public IList<KeyValuePair<string, object>>? MetadataList => Metadata?.ToList();

    public DSCUnitViewModel()
    {
        SelectedSecurityContext = UnitSecurityContext.Default;
        Dependencies = [];
        Settings = [];
        Metadata = [];
    }

    public DSCUnitViewModel(DSCUnitViewModel source)
    {
        CopyFrom(source);
    }

    public DSCUnitViewModel(IDSCUnit unit)
    {
        Unit = unit;
        Id = unit.Id;
        InstanceId = unit.InstanceId;
        Title = GetTitle(unit);
        Description = unit.Description;
        SelectedSecurityContext = UnitSecurityContext.FromEnum(unit.SecurityContext);
        Intent = unit.Intent;
        Dependencies = [..unit.Dependencies.Select(id => new DSCUnitViewModel() { Id = id })];
        Settings = unit.Settings.DeepCopy();
        SettingsJson = unit.Settings.ToJson();
        Metadata = unit.Metadata.DeepCopy();
    }

    private string GetTitle(IDSCUnit unit)
    {
        return unit.ModuleName == string.Empty ? unit.Type : $"{unit.ModuleName}/{unit.Type}";
    }

    public void Validate()
    {
        if (string.IsNullOrEmpty(Title))
        {
            throw new DSCUnitValidationException("Title cannot be null or empty when creating configuration.");
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

        // Add security context metadata if specified and not default.
        if (SelectedSecurityContext != null && SelectedSecurityContext != UnitSecurityContext.Default)
        {
            config.Resources[0].AddSecurityContext(SelectedSecurityContext.Value);
        }

        return config;
    }

    /// <summary>
    /// Loads the details of the configuration unit.
    /// </summary>
    public async Task LoadDetailsAsync()
    {
        if (Unit != null)
        {
            var result = await Unit.GetDetailsAsync();
            Details = new DSCUnitDetailsViewModel(result);
        }
    }

    /// <summary>
    /// Copies the properties from another instance.
    /// </summary>
    /// <param name="source">The source instance.</param>
    public void CopyFrom(DSCUnitViewModel source)
    {
        Unit = source.Unit;
        Id = source.Id;
        InstanceId = source.InstanceId;
        Title = source.Title;
        Description = source.Description;
        SelectedSecurityContext = source.SelectedSecurityContext;
        Intent = source.Intent;
        Dependencies = source.Dependencies?.ToList();
        Metadata = source.Metadata?.DeepCopy();
        SettingsJson = source.SettingsJson;

        // Re-parse the settings JSON to ensure we have a separate instance.
        Settings = string.IsNullOrEmpty(SettingsJson) ? null : DSCPropertySet.FromJsonOrYaml(SettingsJson);
    }

    public void ResolveDependencies(IReadOnlyList<DSCUnitViewModel> availableUnits)
    {
        // If there are no dependencies, nothing to resolve.
        if (Dependencies == null || Dependencies.Count == 0)
        {
            return;
        }

        // Replace each dependency with the matching available unit, if found.
        var resolvedDependencies = new List<DSCUnitViewModel>();
        foreach (var dep in Dependencies)
        {
            var match = availableUnits.FirstOrDefault(u => u.Id == dep.Id);
            resolvedDependencies.Add(match ?? dep);
        }

        // Update the dependencies list.
        Dependencies = resolvedDependencies;
    }

    /// <summary>
    /// Creates a clone of this instance.
    /// </summary>
    /// <returns>A copy of this instance.</returns>
    public DSCUnitViewModel Clone() => new(this);

    [RelayCommand]
    private async Task OnLoadedAsync() => await LoadDetailsAsync();
}
