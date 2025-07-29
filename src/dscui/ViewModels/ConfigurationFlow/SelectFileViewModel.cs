// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using dscui.Contracts.Services;
using dscui.Models;
using DSCUI.Services.DesiredStateConfiguration.Contracts;
using DSCUI.Services.DesiredStateConfiguration.Exceptions;
using DSCUI.Services.DesiredStateConfiguration.Models;
using Microsoft.Extensions.Logging;
using Windows.Storage;

namespace dscui.ViewModels.ConfigurationFlow;

public partial class SelectFileViewModel : ObservableRecipient
{
    private readonly ILogger<SelectFileViewModel> _logger;
    private readonly IStringResource _stringResource;
    private readonly IDSC _dsc;
    private readonly IConfigurationNavigationService _navigationService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviewCommand))]
    [NotifyPropertyChangedFor(nameof(IsFileSelected))]
    private SelectFileResult? _selectFileResult;

    public bool IsFileSelected => SelectFileResult != null;

    private bool CanPreview => SelectFileResult?.Success ?? false;

    public SelectFileViewModel(
        ILogger<SelectFileViewModel> logger,
        IDSC dsc,
        IStringResource stringResource,
        IConfigurationNavigationService navigationService)
    {
        _logger = logger;
        _dsc = dsc;
        _stringResource = stringResource;
        _navigationService = navigationService;
    }

    public async Task SelectFileAsync(StorageFile? file)
    {
        SelectFileResult = await SelectFileInternalAsync(file) ?? SelectFileResult;
    }

    private async Task<SelectFileResult?> SelectFileInternalAsync(StorageFile? file)
    {
        // Check if a file was selected
        if (file == null)
        {
            _logger.LogInformation("No configuration file selected");
            return null;
        }

        try
        {
            _logger.LogInformation($"Selected file: {file.Path}");
            var dscFile = await DSCFile.LoadAsync(file.Path);
            var dscSet = await _dsc.OpenConfigurationSetAsync(dscFile);
            return new()
            {
                Success = true,
                FilePath = file.Path,
                ConfigurationSet = dscSet,
            };
        }
        catch (OpenConfigurationSetException e)
        {
            _logger.LogError(e, $"Opening configuration set failed.");
            var message = GetErrorMessage(e);
            return new SelectFileResult
            {
                Success = false,
                FilePath = file.Path,
                Message = message,
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Unknown error while opening configuration set.");
            var message = _stringResource.GetLocalized("ConfigurationFileOpenUnknownError");
            return new SelectFileResult
            {
                Success = false,
                FilePath = file.Path,
                Message = message,
            };
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

    [RelayCommand(CanExecute = nameof(CanPreview))]
    private async Task OnPreviewAsync()
    {
        var dscSet = SelectFileResult?.ConfigurationSet ?? throw new InvalidOperationException("No configuration set available to preview.");
        _navigationService.NavigateTo<PreviewFileViewModel>(dscSet);
        await Task.CompletedTask;
    }
}
