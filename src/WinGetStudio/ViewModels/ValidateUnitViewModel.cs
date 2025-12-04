// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Management.Configuration;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Exceptions;
using WinGetStudio.Services.DesiredStateConfiguration.Extensions;
using WinGetStudio.Services.DesiredStateConfiguration.Models;
using WingetStudio.Services.VisualFeedback.Contracts;
using WingetStudio.Services.VisualFeedback.Models;

namespace WinGetStudio.ViewModels;

public delegate ValidateUnitViewModel ValidateUnitViewModelFactory();

public sealed partial class ValidateUnitViewModel : ObservableObject, IDisposable
{
    // Tab indices
    private const int NoResultTabIndex = 0;
    private const int YamlOutputTabIndex = 1;
    private const int ErrorOutputTabIndex = 2;

    private readonly IDSC _dsc;
    private readonly IUIFeedbackService _ui;
    private readonly IStringLocalizer<ValidationViewModel> _localizer;
    private readonly ILogger<ValidationViewModel> _logger;
    private readonly IAppFrameNavigationService _navigation;
    private readonly IConfigurationManager _manager;
    private CancellationTokenSource? _cts;
    private bool _disposedValue;

    public ValidateUnitViewModel(
        IDSC dsc,
        IUIFeedbackService ui,
        IStringLocalizer<ValidationViewModel> localizer,
        ILogger<ValidationViewModel> logger,
        UnitViewModelFactory unitFactory,
        IAppFrameNavigationService navigation,
        IConfigurationManager manager)
    {
        _dsc = dsc;
        _ui = ui;
        _localizer = localizer;
        _logger = logger;
        _navigation = navigation;
        _manager = manager;
        Unit = unitFactory();
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    [NotifyCanExecuteChangedFor(nameof(GetCommand))]
    [NotifyCanExecuteChangedFor(nameof(SetCommand))]
    [NotifyCanExecuteChangedFor(nameof(TestCommand))]
    private partial bool CanExecute { get; set; } = true;

    private bool CanCancel => !CanExecute;

    [MemberNotNullWhen(true, nameof(SourceUnit))]
    [MemberNotNullWhen(true, nameof(SourceSet))]
    private bool HasSource => SourceUnit != null && SourceSet != null;

    public bool ShowNoResultState => !ShowYamlOutput && !ShowErrorOutput;

    public bool ShowYamlOutput => !string.IsNullOrWhiteSpace(YamlOutput);

    public bool ShowErrorOutput => !string.IsNullOrWhiteSpace(ErrorOutput);

    public bool CanSaveToSource => HasSource;

    public bool CanGoBackToSource => HasSource;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowYamlOutput))]
    [NotifyPropertyChangedFor(nameof(ShowNoResultState))]
    public partial string? YamlOutput { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowErrorOutput))]
    [NotifyPropertyChangedFor(nameof(ShowNoResultState))]
    public partial string? ErrorOutput { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSaveToSource))]
    [NotifyCanExecuteChangedFor(nameof(SaveToSourceCommand))]
    [NotifyPropertyChangedFor(nameof(CanGoBackToSource))]
    [NotifyCanExecuteChangedFor(nameof(BackToSourceCommand))]
    public partial UnitViewModel? SourceUnit { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSaveToSource))]
    [NotifyCanExecuteChangedFor(nameof(SaveToSourceCommand))]
    [NotifyPropertyChangedFor(nameof(CanGoBackToSource))]
    [NotifyCanExecuteChangedFor(nameof(BackToSourceCommand))]
    public partial SetViewModel? SourceSet { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    public partial UnitViewModel Unit { get; set; }

    [ObservableProperty]
    public partial int SelectedTabIndex { get; set; } = NoResultTabIndex;

    public string Title => string.IsNullOrWhiteSpace(Unit.Title) ? _localizer["ValidateUnit_DefaultTitle"] : Unit.Title;

    [MemberNotNullWhen(true, nameof(SourceUnit))]
    [MemberNotNullWhen(true, nameof(SourceSet))]
    private bool CanSaveToSourceInternal()
    {
        if (!HasSource)
        {
            return false;
        }

        var isSetInPreview = _manager.ActiveSetPreviewState.ActivePreviewSet?.ConfigurationSet == SourceSet;
        if (!isSetInPreview)
        {
            _ui.ShowTimedNotification(_localizer["ValidateUnit_UpdateErrorSetNotInPreviewMessage"], NotificationMessageSeverity.Error);
            return false;
        }

        var isSetBeingApplied = _manager.ActiveSetApplyState.ActiveApplySet != null;
        if (isSetBeingApplied)
        {
            _ui.ShowTimedNotification(_localizer["ValidateUnit_UpdateErrorApplySetInProgressMessage"], NotificationMessageSeverity.Error);
            return false;
        }

        return true;
    }

    private bool CanGoBackToSourceInternal()
    {
        if (!HasSource)
        {
            return false;
        }

        var isSetInPreview = _manager.ActiveSetPreviewState.ActivePreviewSet?.ConfigurationSet == SourceSet;
        if (!isSetInPreview)
        {
            _ui.ShowTimedNotification(_localizer["ValidateUnit_GoBackErrorSetNotInPreviewMessage"], NotificationMessageSeverity.Error);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Retrieves the current configuration unit from the system asynchronously.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecute))]
    private async Task OnGetAsync()
    {
        await RunDscOperationAsync(async (dscUnit, cancellationToken) =>
        {
            var result = await _dsc.GetUnitAsync(dscUnit, cancellationToken);
            if (result.ResultInformation?.IsOk ?? true)
            {
                YamlOutput = result.Settings.ToYaml();
            }

            return result.ResultInformation;
        });
    }

    /// <summary>
    /// Sets the current machine state to the specified configuration unit asynchronously.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecute))]
    private async Task OnSetAsync()
    {
        await RunDscOperationAsync(async (dscUnit, cancellationToken) =>
        {
            var result = await _dsc.SetUnitAsync(dscUnit, cancellationToken);
            return result.ResultInformation;
        });
    }

    /// <summary>
    /// Tests whether the current machine state matches the specified configuration unit asynchronously.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecute))]
    private async Task OnTestAsync()
    {
        await RunDscOperationAsync(async (dscUnit, cancellationToken) =>
        {
            var result = await _dsc.TestUnitAsync(dscUnit, cancellationToken);
            if (result.TestResult == ConfigurationTestResult.Positive)
            {
                _ui.ShowTimedNotification(_localizer["Notification_MachineInDesiredState"], NotificationMessageSeverity.Success);
            }
            else
            {
                _ui.ShowTimedNotification(_localizer["Notification_MachineNotInDesiredState"], NotificationMessageSeverity.Error);
            }

            return result.ResultInformation;
        });
    }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void OnCancel()
    {
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }
    }

    [RelayCommand(CanExecute = nameof(CanGoBackToSource))]
    private void OnBackToSource()
    {
        if (CanGoBackToSourceInternal())
        {
            _navigation.NavigateTo<ConfigurationViewModel>();
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveToSource))]
    private async Task OnSaveToSourceAsync()
    {
        if (_manager.ActiveSetPreviewState.ActivePreviewSet == null)
        {
            return;
        }

        try
        {
            _ui.ShowTaskProgress();
            _logger.LogInformation($"Attempting to save changes to source unit");
            if (CanSaveToSourceInternal())
            {
                await _manager.ActiveSetPreviewState.ActivePreviewSet.UpdateUnitAsync(SourceUnit, Unit);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Updating configuration unit failed");
            _ui.ShowTimedNotification(_localizer["ValidateUnit_UpdateErrorMessage", ex.Message], NotificationMessageSeverity.Error);
        }
        finally
        {
            _ui.HideTaskProgress();
        }
    }

    /// <summary>
    /// Runs a DSC operation while managing UI feedback.
    /// </summary>
    /// <param name="action">The DSC operation to execute.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task RunDscOperationAsync(Func<IDSCUnit, CancellationToken, Task<IDSCUnitResultInformation?>> action)
    {
        try
        {
            CanExecute = false;
            ResetOutput();
            _cts = new CancellationTokenSource();
            _ui.ShowTaskProgress();
            var unit = await CreateUnitAsync();
            var result = await action(unit, _cts.Token);
            if (result != null && !result.IsOk)
            {
                var title = _localizer["ErrorCodeText", $"0x{result.ResultCode.HResult:X}"];
                List<string> messageList = [result.Description, result.Details];
                var message = string.Join(Environment.NewLine, messageList.Where(s => !string.IsNullOrEmpty(s)));
                _ui.ShowTimedNotification(title, message, NotificationMessageSeverity.Error);
                ErrorOutput = title + Environment.NewLine + Environment.NewLine + message;
            }
        }
        catch (OpenConfigurationSetException ex)
        {
            _logger.LogError(ex, "An error occurred while opening the DSC configuration set.");
            _ui.ShowTimedNotification(ex.GetErrorMessage(_localizer), NotificationMessageSeverity.Error);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogInformation(ex, "Operation canceled.");
            _ui.ShowTimedNotification(_localizer["ValidateUnit_OperationCanceledMessage"], NotificationMessageSeverity.Warning);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while executing a DSC operation.");
            _ui.ShowTimedNotification(ex.Message, NotificationMessageSeverity.Error);
        }
        finally
        {
            _ui.HideTaskProgress();
            _cts?.Dispose();
            _cts = null;
            CanExecute = true;
        }
    }

    /// <summary>
    /// Creates a DSC unit from the current state.
    /// </summary>
    /// <returns>The created DSC unit.</returns>
    private async Task<IDSCUnit> CreateUnitAsync()
    {
        var config = await Unit.ToConfigurationV3Async();
        var dscFile = DSCFile.CreateVirtual(config.ToYaml());
        var dscSet = await _dsc.OpenConfigurationSetAsync(dscFile);
        return dscSet.Units[0];
    }

    private void OnUnitPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(UnitViewModel.Title))
        {
            OnPropertyChanged(nameof(Title));
        }
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _cts?.Dispose();
                _cts = null;
                Unit.PropertyChanged -= OnUnitPropertyChanged;
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void ResetOutput()
    {
        YamlOutput = null;
        ErrorOutput = null;
        SelectedTabIndex = NoResultTabIndex;
    }

    partial void OnYamlOutputChanged(string? oldValue, string? newValue)
    {
        if (ShowYamlOutput)
        {
            SelectedTabIndex = YamlOutputTabIndex;
        }
    }

    partial void OnErrorOutputChanged(string? oldValue, string? newValue)
    {
        if (ShowErrorOutput)
        {
            SelectedTabIndex = ErrorOutputTabIndex;
        }
    }

    partial void OnUnitChanged(UnitViewModel oldValue, UnitViewModel newValue)
    {
        if (oldValue != null)
        {
            oldValue.PropertyChanged -= OnUnitPropertyChanged;
        }

        if (newValue != null)
        {
            newValue.PropertyChanged += OnUnitPropertyChanged;
        }
    }
}
