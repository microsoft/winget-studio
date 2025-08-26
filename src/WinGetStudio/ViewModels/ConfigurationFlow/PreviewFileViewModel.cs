// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinGetStudio.Common.Windows.FileDialog;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Contracts.ViewModels;
using WinGetStudio.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Exceptions;
using WinGetStudio.Services.DesiredStateConfiguration.Models;
using WinRT.Interop;

namespace WinGetStudio.ViewModels.ConfigurationFlow;

public partial class PreviewFileViewModel : ObservableRecipient, INavigationAware
{
    private readonly IConfigurationNavigationService _navigationService;
    private readonly IDSC _dsc;
    private readonly IDSCSetBuilder _dscSetBuilder;
    private readonly IStringResource _stringResource;
    private readonly ILogger<PreviewFileViewModel> _logger;
    private IDSCSet? _dscSet;
    private string _yaml = string.Empty;

    public ObservableCollection<DSCConfigurationUnitViewModel> ConfigurationUnits { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditPanelVisible))]
    public partial DSCConfigurationUnitViewModel? SelectedUnit { get; set; }

    [ObservableProperty]
    public partial bool LoadingUnits { get; set; } = true;

    [ObservableProperty]
    public partial string FilePath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsInEditMode { get; set; }

    [ObservableProperty]
    public partial bool IsStateChanged { get; set; } = false;

    public bool CanApply => ConfigurationUnits.Count > 0 && !IsStateChanged;

    public bool IsEditPanelVisible => SelectedUnit != null;

    partial void OnIsInEditModeChanging(bool value)
    {
        _ = UpdateUnits();
    }

    partial void OnIsStateChangedChanged(bool value)
    {
        OnPropertyChanged(nameof(CanApply));
    }

    private async Task UpdateUnits()
    {
        LoadingUnits = true;
        if (IsInEditMode)
        {
            foreach (var u in ConfigurationUnits)
            {
                u.UpdateUnit();
                if (u.ConfigurationUnit is EditableDSCUnit editableUnit)
                {
                    _dscSetBuilder.UpdateUnit(editableUnit);
                }
            }

            ConfigurationUnits.Clear();
            var dscSet = await _dscSetBuilder.BuildAsync();
            foreach (var u in dscSet.Units)
            {
                ConfigurationUnits.Add(new(u));
            }

            _dsc.GetConfigurationUnitDetails(dscSet);
        }
        else
        {
            ConfigurationUnits.Clear();
            foreach (var u in _dscSetBuilder.Units)
            {
                ConfigurationUnits.Add(new(u));
            }
        }

        LoadingUnits = false;
    }

    partial void OnSelectedUnitChanged(DSCConfigurationUnitViewModel? oldValue, DSCConfigurationUnitViewModel? newValue)
    {
        OnPropertyChanged(nameof(IsEditPanelVisible));
        IsStateChanged = true;
    }

    public PreviewFileViewModel(IConfigurationNavigationService navigationService, IDSC dsc, IDSCSetBuilder setBuilder, IStringResource stringResource, ILogger<PreviewFileViewModel> logger)
    {
        _navigationService = navigationService;
        _dsc = dsc;
        _dscSetBuilder = setBuilder;
        _stringResource = stringResource;
        _logger = logger;
        ConfigurationUnits.CollectionChanged += (_, __) =>
        {
            OnPropertyChanged(nameof(CanApply));
        };
    }

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is ConfigurationPageParameter configParameter)
        {
            if (configParameter.DSCSet != null)
            {
                _dsc.GetConfigurationUnitDetails(configParameter.DSCSet);
                _dscSetBuilder.ImportSet(configParameter.DSCSet);
                _dscSet = configParameter.DSCSet;
            }
            else
            {
                _dscSet = new EditableDSCSet();
            }

            if (configParameter.ResetDSCSet)
            {
                _dscSetBuilder.ClearUnits();
                ConfigurationUnits.Clear();
            }

            _ = UpdateUnits();
        }
        else if (parameter is string filePath)
        {
            _ = ImportDSCSetFromPathAsync(filePath);
        }
        else
        {
            _ = UpdateUnits();
        }

        FilePath = _dscSetBuilder.TargetFilePath;
    }

    public void OnNavigatedFrom()
    {
        // No-op
    }

    private async Task ImportDSCSetFromPathAsync(string path)
    {
        var dscFile = await DSCFile.LoadAsync(path);
        var dscSet = await _dsc.OpenConfigurationSetAsync(dscFile);
        FilePath = path;
        _dscSetBuilder.ImportSet(dscSet);
        _dscSet = dscSet;
        _dscSetBuilder.TargetFilePath = path;

        _ = UpdateUnits();
    }

    public async Task<bool> IsSaveRequiredAsync()
    {
        await UpdateUnits();
        if (_yaml == await _dscSetBuilder.ConvertToYamlAsync())
        {
            return false;
        }

        return true;
    }

    [RelayCommand]
    private async Task OnStoreYamlStateAsync()
    {
        _dscSet = await _dscSetBuilder.BuildAsync();
        _yaml = await _dscSetBuilder.ConvertToYamlAsync();
    }

    [RelayCommand]
    private async Task OnApplyAsync()
    {
        if (!_dscSetBuilder.IsEmpty())
        {
            _navigationService.NavigateTo<ApplyFileViewModel>(await _dscSetBuilder.BuildAsync());
        }
    }

    [RelayCommand]
    private async Task OnSaveAsync()
    {
        await UpdateUnits();
        var yaml = await _dscSetBuilder.ConvertToYamlAsync();
        if (FilePath == string.Empty)
        {
            await SaveAsCommand.ExecuteAsync(null);
        }
        else
        {
            File.WriteAllText(FilePath, yaml);
        }

        _yaml = yaml;
        IsStateChanged = false;
    }

    [RelayCommand]
    private async Task OnSaveAsAsync()
    {
        await UpdateUnits();
        var yaml = await _dscSetBuilder.ConvertToYamlAsync();

        var picker = new FileSavePicker();

        // Get the HWND of the current window
        var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
        InitializeWithWindow.Initialize(picker, hwnd);

        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.SuggestedFileName = "configuration.winget";
        picker.FileTypeChoices.Add("Winget Configuration File", new List<string>() { ".winget" });

        StorageFile file = await picker.PickSaveFileAsync();
        if (file != null)
        {
            _dscSetBuilder.TargetFilePath = file.Path;
            FilePath = file.Path;
            await FileIO.WriteTextAsync(file, yaml);
        }

        _yaml = yaml;
        IsStateChanged = false;
    }

    [RelayCommand]
    private void OnDiscard()
    {
        _dscSetBuilder.ImportSet(_dscSet);
    }

    [RelayCommand]
    private async Task OnAddResourceAsync()
    {
        EditableDSCUnit u = new();
        _dscSetBuilder.AddUnit(u);
        ConfigurationUnits.Add(new(u));
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OnRemoveResourceAsync(DSCConfigurationUnitViewModel context)
    {
        if (context.ConfigurationUnit is EditableDSCUnit editableUnit)
        {
            _dscSetBuilder.RemoveUnit(editableUnit);
            ConfigurationUnits.Remove(context);
            if (SelectedUnit != null && context.InstanceId == SelectedUnit.InstanceId)
            {
                SelectedUnit = null;
            }
        }

        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OnBrowseAsync()
    {
        var fileDialog = new WindowOpenFileDialog();
        fileDialog.AddFileType("YAML files", ".yaml", ".yml", ".winget");
        var file = await fileDialog.ShowAsync(App.MainWindow);

        // Check if a file was selected
        if (file == null)
        {
            return;
        }

        try
        {
            await ImportDSCSetFromPathAsync(file.Path);
        }
        catch (OpenConfigurationSetException e)
        {
            _logger.LogError(e, $"Opening configuration set failed.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Unknown error while opening configuration set.");
        }
    }

    private string GetErrorMessage(OpenConfigurationSetException exception)
    {
        switch (exception.ResultCode.HResult)
        {
            case ConfigurationException.WingetConfigErrorInvalidFieldType:
                return _stringResource.GetLocalized("ConfigurationFieldInvalidType", exception.Field);
            case ConfigurationException.WingetConfigErrorInvalidFieldValue:
                return _stringResource.GetLocalized("ConfigurationFieldInvalidValue", exception.Field, exception.Value);
            case ConfigurationException.WingetConfigErrorMissingField:
                return _stringResource.GetLocalized("ConfigurationFieldMissing", exception.Field);
            case ConfigurationException.WingetConfigErrorUnknownConfigurationFileVersion:
                return _stringResource.GetLocalized("ConfigurationFileVersionUnknown", exception.Value);
            case ConfigurationException.WingetConfigErrorInvalidConfigurationFile:
            case ConfigurationException.WingetConfigErrorInvalidYaml:
            default:
                return _stringResource.GetLocalized("ConfigurationFileInvalid");
        }
    }
}
