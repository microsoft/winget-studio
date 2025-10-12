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
    private readonly IDSCUnit _unit;

    [ObservableProperty]
    public partial DSCUnitDetailsViewModel? Details { get; set; }

    public string Id { get; set; }

    public Guid InstanceId { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; }

    [ObservableProperty]
    public partial string Description { get; set; }

    public bool RequiresElevation { get; set; }

    public string Intent { get; set; }

    public IList<string> Dependencies { get; }

    public IList<KeyValuePair<string, object>> Settings { get; set; }

    public IList<KeyValuePair<string, string>> Metadata { get; set; }

    public DSCUnitViewModel(IDSCUnit unit)
    {
        _unit = unit;

        // Initialize properties from the unit
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
        var config = new ConfigurationV3()
        {
            Resources =
            [
                new()
                {
                    Name = $"{Title}-0",
                    Type = Title,
                }
            ],
        };
        config.AddWinGetMetadata();
        return config;
    }

    [RelayCommand]
    private async Task OnLoadedAsync()
    {
        var result = await _unit.GetDetailsAsync();
        Details = new DSCUnitDetailsViewModel(result);
    }
}
