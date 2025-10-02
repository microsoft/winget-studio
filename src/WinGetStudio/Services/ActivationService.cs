// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinGetStudio.Activation;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Services.Settings.Contracts;
using WinGetStudio.Views;

namespace WinGetStudio.Services;

public class ActivationService : IActivationService
{
    private readonly ActivationHandler<Windows.ApplicationModel.Activation.LaunchActivatedEventArgs> _defaultHandler;
    private readonly IEnumerable<IActivationHandler> _activationHandlers;
    private readonly IAppSettingsService _appSettings;
    private readonly IUserSettings _userSettings;

    private UIElement? _shell;
    private bool _isInitialActivation = true;

    public ActivationService(
        ActivationHandler<Windows.ApplicationModel.Activation.LaunchActivatedEventArgs> defaultHandler,
        IEnumerable<IActivationHandler> activationHandlers,
        IUserSettings userSettings,
        IAppSettingsService appSettings)
    {
        _defaultHandler = defaultHandler;
        _activationHandlers = activationHandlers;
        _userSettings = userSettings;
        _appSettings = appSettings;
    }

    public async Task ActivateAsync(object activationArgs)
    {
        if (_isInitialActivation)
        {
            _isInitialActivation = false;

            // Execute tasks before activation.
            await InitializeAsync();

            // Set the MainWindow Content.
            if (App.MainWindow.Content == null)
            {
                _shell = App.GetService<ShellPage>();
                App.MainWindow.Content = _shell ?? new Frame();
            }

            // Activate the MainWindow.
            App.MainWindow.Activate();

            // Execute tasks after activation.
            await StartupAsync();
        }

        // Handle activation via ActivationHandlers.
        await HandleActivationAsync(activationArgs);
    }

    private async Task HandleActivationAsync(object activationArgs)
    {
        var activationHandler = _activationHandlers.FirstOrDefault(h => h.CanHandle(activationArgs));

        if (activationHandler != null)
        {
            await activationHandler.HandleAsync(activationArgs);
        }

        if (_defaultHandler.CanHandle(activationArgs))
        {
            await _defaultHandler.HandleAsync(activationArgs);
        }
    }

    private async Task InitializeAsync()
    {
        // Initialize the user settings (JSON file).
        await _userSettings.InitializeAsync().ConfigureAwait(false);
    }

    private async Task StartupAsync()
    {
        // Apply settings to the app (theme, etc.).
        await _appSettings.ApplySettingsAsync().ConfigureAwait(false);
    }
}
