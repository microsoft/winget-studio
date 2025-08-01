using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Contracts.ViewModels;
using WinGetStudio.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using Windows.Foundation.Collections;

namespace WinGetStudio.ViewModels;

public delegate ValidationViewModel ValidationViewModelFactory();

public partial class ValidationViewModel : ObservableRecipient, INavigationAware
{
    private readonly IDSC _dsc;

    [ObservableProperty]
    public partial string ModuleName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Type { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string RawData { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TabHeader { get; set; } = "New Tab";

    [ObservableProperty]
    public partial bool ActionsEnabled { get; set; } = true;

    [ObservableProperty]
    public partial bool TestBannerVisible { get; set; } = false;

    [ObservableProperty]
    public partial bool TestResult { get; set; } = false;

    [ObservableProperty]
    public partial string TestBannerText { get; set; } = string.Empty;
    public bool IsPropertiesEmpty => Properties.Count == 0;
    public ObservableCollection<ConfigurationProperty> Properties { get; } = new();
    
    public ValidationViewModel(IDSC dsc, IDSCSetBuilder setBuilder, IAppNavigationService navService)
    {
        _dsc = dsc;

        Properties.CollectionChanged += (_, __) =>
        {
            OnPropertyChanged(nameof(IsPropertiesEmpty));
        };
    }

    public void OnNavigatedTo(object parameter)
    {
        if(parameter is IDSCUnit u)
        {
            ModuleName = u.ModuleName;
            Type = u.Type;
            Title = ModuleName == string.Empty ? Type : $"{ModuleName}/{Type}";

            foreach (var kvp in u.Settings)
            {
                if (kvp.Value is string s)
                {
                    Properties.Add(new(kvp.Key, new StringValue(s)));
                }
                else if (kvp.Value is bool b)
                {
                    Properties.Add(new(kvp.Key, new BooleanValue(b)));
                }
                else if (kvp.Value is double d)
                {
                    Properties.Add(new(kvp.Key, new NumberValue(d)));
                }
                else if (kvp.Value is ValueSet v)
                {
                    ObservableCollection<ConfigurationProperty> nestedProperties = new();
                    ConvertYamlToUIHelper(nestedProperties, v);
                    Properties.Add(new(kvp.Key, new ObjectValue(nestedProperties)));
                }
            }
        }
    }

    public void OnNavigatedFrom()
    {
        // No-op
    }

    /// <summary>
    /// Creates a new <see cref="ConfigurationUnitModel"/> instance based on the current module name and properties.
    /// </summary>
    /// <returns></returns>
    private ConfigurationUnitModel CreateConfigurationUnitModel()
    {
        ConfigurationUnitModel unit = new();
        unit.Type = Title;
        ConfigurationPropertiesToValueSet(unit.Settings, Properties);
        return unit;
    }
    private void ConfigurationPropertiesToValueSet(ValueSet settings, ObservableCollection<ConfigurationProperty> properties)
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
                ValueSet nestedSettings = new();
                ConfigurationPropertiesToValueSet(nestedSettings, nestedProperties);
                settings.Add(new(property.Name, nestedSettings));
            }
        }
    }
    partial void OnModuleNameChanged(string oldValue, string newValue)
    {
        TabHeader = newValue;
    }

    /// <summary>
    /// Converts YAML data into a user interface representation asynchronously.
    /// </summary>
    /// <remarks>This method processes the raw YAML data and updates the UI-related properties accordingly. 
    /// It clears the existing properties and populates them based on the parsed YAML configuration.</remarks>
    /// <returns></returns>
    [RelayCommand]
    private async Task OnConvertYamlToUIAsync()
    {
        var unit = CreateConfigurationUnitModel();
        if (unit.TryLoad(RawData))
        {
            Title = unit.Type;
            ModuleName = Title.Split("/").First();
            Type = Title.Split("/").Last();

            Properties.Clear();

            ConvertYamlToUIHelper(Properties, unit.Settings);
        }
        else
        {
            //TODO implement error handling
        }
    }

    /// <summary>
    /// Helper function to convert a <see cref="ValueSet"/> into an observable collection of configuration properties.
    /// </summary>
    /// <remarks>This method recursively processes nested <see cref="ValueSet"/> instances, converting them
    /// into collections of configuration properties. Each key-value pair in the <paramref name="settings"/> is mapped
    /// to a corresponding <see cref="ConfigurationProperty"/> based on the type of the value.</remarks>
    /// <param name="properties">The collection to populate with configuration properties derived from the <paramref name="settings"/>.</param>
    /// <param name="settings">A <see cref="ValueSet"/> containing key-value pairs to be converted into configuration properties. The values
    /// can be of type <see langword="string"/>, <see langword="bool"/>, <see langword="double"/>, or nested <see
    /// cref="ValueSet"/>.</param>
    private void ConvertYamlToUIHelper(ObservableCollection<ConfigurationProperty> properties, ValueSet settings)
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
            }else if (kvp.Value is ValueSet v)
            {
                ObservableCollection<ConfigurationProperty> nestedProperties = new();
                ConvertYamlToUIHelper(nestedProperties, v);
                properties.Add(new(kvp.Key, new ObjectValue(nestedProperties)));
            }
        }
    }

    /// <summary>
    /// Retrieves the current configuration unit from the system asynchronously.
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task OnGetAsync()
    {
        ActionsEnabled = false;
        ConfigurationUnitModel unit = CreateConfigurationUnitModel();
        await _dsc.Get(unit);
        RawData = unit.ToYaml();
        ActionsEnabled = true;
    }

    /// <summary>
    /// Sets the current machine state to the specified configuration unit asynchronously.
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task OnSetAsync()
    {
        ActionsEnabled = false;
        ConfigurationUnitModel unit = CreateConfigurationUnitModel();
        await _dsc.Set(unit);
        ActionsEnabled = true;
    }

    /// <summary>
    /// Tests whether the current machine state matches the specified configuration unit asynchronously.
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task OnTestAsync()
    {
        ActionsEnabled = false;
        ConfigurationUnitModel unit = CreateConfigurationUnitModel();
        await _dsc.Test(unit);
        TestResult = unit.TestResult;
        if (unit.TestResult)
        {
            TestBannerText = "Machine is in desired state";
        }
        else
        {
            TestBannerText = "Machine is not in desired state";
        }
        TestBannerVisible = true;
        ActionsEnabled = true;
    }

    /// <summary>
    /// Exports the current configuration asynchronously and updates the raw data representation.
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task OnExportAsync()
    {
        ActionsEnabled = false;
        ConfigurationUnitModel unit = CreateConfigurationUnitModel();
        await _dsc.Export(unit);
        RawData = unit.ToYaml();
        ActionsEnabled = true;
    }
}
