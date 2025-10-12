// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Windows.Storage;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Exceptions;
using WinGetStudio.Services.DesiredStateConfiguration.Models;
using WingetStudio.Services.VisualFeedback.Contracts;
using WingetStudio.Services.VisualFeedback.Models;

namespace WinGetStudio.ViewModels.ConfigurationFlow;

public partial class PreviewFileViewModel : ObservableRecipient
{
    private readonly ILogger<PreviewFileViewModel> _logger;
    private readonly IStringLocalizer<PreviewFileViewModel> _localizer;
    private readonly IUIFeedbackService _ui;
    private readonly IDSC _dsc;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmptyState))]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmptyState))]
    public partial ObservableCollection<DSCUnitViewModel>? ConfigurationUnits { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsUnitSelected))]
    public partial DSCUnitViewModel? SelectedUnit { get; set; }

    public bool IsUnitSelected => SelectedUnit != null;

    [ObservableProperty]
    public partial bool IsEditMode { get; set; }

    public bool IsEmptyState => !IsLoading && ConfigurationUnits == null;

    public PreviewFileViewModel(
        ILogger<PreviewFileViewModel> logger,
        IStringLocalizer<PreviewFileViewModel> localizer,
        IUIFeedbackService ui,
        IDSC dsc)
    {
        _logger = logger;
        _localizer = localizer;
        _ui = ui;
        _dsc = dsc;
    }

    /// <summary>
    /// Opens the configuration file.
    /// </summary>
    /// <param name="file">The configuration file to open.</param>
    public async Task OpenConfigurationFileAsync(StorageFile file)
    {
        try
        {
            _logger.LogInformation($"Selected file: {file.Path}");
            ClearConfigurationSet();
            IsLoading = true;
            var dscFile = await DSCFile.LoadAsync(file.Path);
            var dscSet = await _dsc.OpenConfigurationSetAsync(dscFile);
            _dsc.GetConfigurationUnitDetails(dscSet);
            ShowConfigurationSet(dscSet);
        }
        catch (OpenConfigurationSetException ex)
        {
            _logger.LogError(ex, $"Opening configuration set failed");
            _ui.ShowTimedNotification(ex.GetErrorMessage(_localizer), NotificationMessageSeverity.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unknown error while opening configuration set");
            _ui.ShowTimedNotification(ex.Message, NotificationMessageSeverity.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task OnSaveAsync()
    {
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OnSaveAsAsync()
    {
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OnApplyAsync()
    {
        await Task.CompletedTask;
    }

    [RelayCommand]
    private void OnValidateUnit(DSCUnitViewModel unit)
    {
        // TODO
    }

    [RelayCommand]
    private void OnEditUnit(DSCUnitViewModel unit)
    {
        SelectedUnit = unit;
    }

    [RelayCommand]
    private void OnDeleteUnit(DSCUnitViewModel unit)
    {
        if (SelectedUnit == unit)
        {
            SelectedUnit = null;
        }

        ConfigurationUnits?.Remove(unit);
    }

    [RelayCommand]
    private void OnAddUnit()
    {
        // TODO
    }

    /// <summary>
    /// Shows the configuration set.
    /// </summary>
    /// <param name="dscSet">The configuration set to show.</param>
    private void ShowConfigurationSet(IDSCSet? dscSet)
    {
        ConfigurationUnits = dscSet == null ? null : new(dscSet.Units.Select(unit => new DSCUnitViewModel(unit)));
        SelectedUnit = null;
    }

    /// <summary>
    /// Clears the current configuration set.
    /// </summary>
    private void ClearConfigurationSet() => ShowConfigurationSet(null);
}
