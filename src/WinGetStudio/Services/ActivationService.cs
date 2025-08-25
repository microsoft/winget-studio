// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinGetStudio.Activation;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Views;

namespace WinGetStudio.Services;

public class ActivationService : IActivationService
{
    private readonly ActivationHandler<Windows.ApplicationModel.Activation.LaunchActivatedEventArgs> _defaultHandler;
    private readonly IEnumerable<IActivationHandler> _activationHandlers;
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ILocalSettingsService _localSettingsService;
    private UIElement? _shell;
    private bool _isInitialActivation = true;

    public ActivationService(
        ActivationHandler<Windows.ApplicationModel.Activation.LaunchActivatedEventArgs> defaultHandler,
        IEnumerable<IActivationHandler> activationHandlers,
        IThemeSelectorService themeSelectorService,
        ILocalSettingsService localSettingsService)
    {
        _defaultHandler = defaultHandler;
        _activationHandlers = activationHandlers;
        _themeSelectorService = themeSelectorService;
        _localSettingsService = localSettingsService;
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
        await _themeSelectorService.InitializeAsync().ConfigureAwait(false);
        await Task.CompletedTask;
    }

    private async Task StartupAsync()
    {
        await _themeSelectorService.SetRequestedThemeAsync();
        await Task.CompletedTask;
    }
}
