// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Extensions;
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

    public bool RequiresElevation { get; set; }

    public string? Intent { get; set; }

    public IList<string>? Dependencies { get; set; }

    public IList<KeyValuePair<string, object>>? Settings { get; set; }

    public IList<KeyValuePair<string, string>>? Metadata { get; set; }

    public DSCUnitViewModel(DSCUnitViewModel source)
    {
        // TODO Deep copy members

        Unit = source.Unit;
        Id = source.Id;
        InstanceId = source.InstanceId;
        Title = source.Title;
        Description = source.Description;
        RequiresElevation = source.RequiresElevation;
        Intent = source.Intent;
        Dependencies = source.Dependencies;
        Settings = source.Settings;
        Metadata = source.Metadata;
    }

    public DSCUnitViewModel(IDSCUnit unit)
    {
        // TODO Deep copy members

        Unit = unit;
        Id = unit.Id;
        InstanceId = unit.InstanceId;
        Title = GetTitle(unit);
        Description = unit.Description;
        RequiresElevation = unit.RequiresElevation;
        Intent = unit.Intent;
        Dependencies = unit.Dependencies;
        Settings = unit.Settings;
        Metadata = unit.Metadata;
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
        var properties = Settings?.Count == 0 ? null : Settings?.ToDictionary(kv => kv.Key, kv => kv.Value);
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

    [RelayCommand]
    private async Task OnLoadedAsync() => await LoadDetailsAsync();
}
