// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Models;
using Windows.Foundation.Collections;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace WinGetStudio.ViewModels;

public partial class DSCConfigurationUnitViewModel : ObservableObject
{
    public readonly IDSCUnit ConfigurationUnit;
    private readonly IAppNavigationService _navigationService;
    private readonly IDSCSetBuilder _dscSetBuilder;

    [ObservableProperty]
    private DSCConfigurationUnitDetailsViewModel _details;

    public string Id => ConfigurationUnit.Id;

    public Guid InstanceId => ConfigurationUnit.InstanceId;

    public string Title => GetTitle();

    [ObservableProperty]
    public partial string Type { get; set; }

    [ObservableProperty]
    public partial string Description { get; set; }

    [ObservableProperty]
    public partial bool RequiresElevation { get; set; }

    [ObservableProperty]
    public partial string Intent { get; set; }

    [ObservableProperty]
    public partial string ModuleName { get; set; }

    public IList<string> Dependencies => ConfigurationUnit.Dependencies;

    [ObservableProperty]
    public partial IList<KeyValuePair<string, object>> Settings { get; set; }

    public IList<KeyValuePair<string, string>> Metadata => ConfigurationUnit.Metadata;

    public ObservableCollection<ConfigurationProperty> Properties { get; } = new();

    [ObservableProperty]
    public partial string SettingsString { get; set; }

    public DSCConfigurationUnitViewModel(IDSCUnit configurationUnit)
    {
        ConfigurationUnit = configurationUnit;
        _navigationService = App.GetService<IAppNavigationService>();
        Type = configurationUnit.Type;
        Description = configurationUnit.Description;
        RequiresElevation = configurationUnit.RequiresElevation;
        Intent = configurationUnit.Intent;
        ModuleName = configurationUnit.ModuleName;
        Settings = configurationUnit.Settings;
        ConvertKeyValueListToProperties(Properties, Settings);
        _dscSetBuilder = App.GetService<IDSCSetBuilder>();

        var dict = Settings.ToDictionary();

        var serializer = new SerializerBuilder()
         .WithNamingConvention(CamelCaseNamingConvention.Instance)
         .Build();

        SettingsString = serializer.Serialize(dict);
    }


    partial void OnTypeChanged(string oldValue, string newValue)
    {
        OnPropertyChanged(nameof(Title));
    }

    partial void OnModuleNameChanged(string oldValue, string newValue)
    {
        OnPropertyChanged(nameof(Title));
    }


    private void ConvertKeyValueListToProperties(ObservableCollection<ConfigurationProperty> properties, IList<KeyValuePair<string, object>> settings)
    {
        foreach (var kvp in settings)
        {
            if (kvp.Value is string s)
            {
                properties.Add(new(kvp.Key, new StringValue(s)));
            }
            else if (kvp.Value is bool b)
            {
                properties.Add(new(kvp.Key, new BooleanValue(b)));
            }
            else if (kvp.Value is double d)
            {
                properties.Add(new(kvp.Key, new NumberValue(d)));
            }
            else if (kvp.Value is ValueSet v)
            {
                ObservableCollection<ConfigurationProperty> nestedProperties = new();
                ConvertKeyValueListToProperties(nestedProperties, v.ToList());
                properties.Add(new(kvp.Key, new ObjectValue(nestedProperties)));
            }
        }
    }

    public void UpdateUnit()
    {
        if(ConfigurationUnit is EditableDSCUnit e)
        {
            e.RequiresElevation = RequiresElevation;
            e.Description = Description;
            e.ModuleName = ModuleName;
            e.Type = Type;
            Settings.Clear();
            ConfigurationPropertiesToKeyValueList(Settings, Properties);
            e.Settings = Settings;
        }
    }

    private void ConfigurationPropertiesToKeyValueList(IList<KeyValuePair<string, object>> settings, ObservableCollection<ConfigurationProperty> properties)
    {
        foreach (var property in properties)
        {
            if (property.Value.Value is string s)
            {
                settings.Add(new(property.Name, s));
            }
            else if (property.Value.Value is bool b)
            {
                settings.Add(new(property.Name, b));
            }
            else if (property.Value.Value is double d)
            {
                settings.Add(new(property.Name, d));
            }
            else if (property.Value.Value is ObservableCollection<ConfigurationProperty> nestedProperties)
            {
                List<KeyValuePair<string, object>> nestedSettings = new();
                ConfigurationPropertiesToKeyValueList(nestedSettings, nestedProperties);
                settings.Add(new(property.Name, nestedSettings));
            }
        }
    }

    private string GetTitle()
    {
        return ModuleName == string.Empty ? Type : $"{ModuleName}/{Type}";
    }

    [RelayCommand]
    private async Task OnLoadedAsync()
    {
        var result = await ConfigurationUnit.GetDetailsAsync();
        Details = new DSCConfigurationUnitDetailsViewModel(result);
    }

    [RelayCommand]
    private async Task OnValidateAsync()
    {
        _navigationService.NavigateTo<ValidationFrameViewModel>(ConfigurationUnit);
        await Task.CompletedTask;
    }
}
