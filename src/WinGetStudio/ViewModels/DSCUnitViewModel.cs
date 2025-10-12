// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Extensions;
using WinGetStudio.Services.DesiredStateConfiguration.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Models.Schemas.ConfigurationV3;

namespace WinGetStudio.ViewModels;

public partial class DSCUnitViewModel : ObservableObject
{
    public IDSCUnit? Unit { get; set; }

    [ObservableProperty]
    public partial DSCUnitDetailsViewModel? Details { get; set; }

    public string? Id { get; set; }

    public Guid? InstanceId { get; set; }

    [ObservableProperty]
    public partial string? Title { get; set; }

    [ObservableProperty]
    public partial string? Description { get; set; }

    [ObservableProperty]
    public partial bool RequiresElevation { get; set; }

    [ObservableProperty]
    public partial string? Intent { get; set; }

    [ObservableProperty]
    public partial IList<string>? Dependencies { get; set; }

    [ObservableProperty]
    public partial DSCPropertySet? Settings { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MetadataList))]
    public partial DSCPropertySet? Metadata { get; set; }

    [ObservableProperty]
    public partial string? SettingsJson { get; set; }

    public IList<KeyValuePair<string, object>>? MetadataList => Metadata?.ToList();

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
        RequiresElevation = unit.RequiresElevation;
        Intent = unit.Intent;
        Dependencies = [..unit.Dependencies];
        Settings = unit.Settings.DeepCopy();
        SettingsJson = unit.Settings.ToJson();
        Metadata = unit.Metadata.DeepCopy();
    }

    private string GetTitle(IDSCUnit unit)
    {
        return unit.ModuleName == string.Empty ? unit.Type : $"{unit.ModuleName}/{unit.Type}";
    }

    /// <summary>
    /// Creates a configuration object representing this DSC unit.
    /// </summary>
    /// <returns>The configuration object.</returns>
    public ConfigurationV3 ToConfigurationV3()
    {
        if (string.IsNullOrEmpty(Title))
        {
            throw new InvalidOperationException("Title cannot be null or empty when creating configuration.");
        }

        var dependencies = Dependencies?.Count == 0 ? null : Dependencies?.ToList();
        var properties = Settings?.Count == 0 ? null : Settings?.DeepCopy();
        var metadata = Metadata?.Count == 0 ? null : Metadata?.DeepCopy();
        var additionalProperties = metadata == null ? [] : new Dictionary<string, object> { { "metadata", metadata } };
        var config = new ConfigurationV3()
        {
            Resources =
            [
                new()
                {
                    Name = $"{Title}-0",
                    Type = Title,
                    DependsOn = dependencies,
                    Properties = properties,
                    AdditionalProperties = additionalProperties,
                }
            ],
        };
        config.AddWinGetMetadata();
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
        RequiresElevation = source.RequiresElevation;
        Intent = source.Intent;
        Dependencies = source.Dependencies;
        Metadata = source.Metadata?.DeepCopy();
        SettingsJson = source.SettingsJson;

        // Re-parse the settings JSON to ensure we have a separate instance.
        Settings = DSCPropertySet.FromJsonOrYaml(SettingsJson);
    }

    /// <summary>
    /// Creates a clone of this instance.
    /// </summary>
    /// <returns>A copy of this instance.</returns>
    public DSCUnitViewModel Clone() => new(this);

    [RelayCommand]
    private async Task OnLoadedAsync() => await LoadDetailsAsync();
}
