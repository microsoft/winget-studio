// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using WinGetStudio.Activation;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Extensions;
using WinGetStudio.Models;
using WinGetStudio.Services;
using WinGetStudio.Services.Core.Extensions;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Extensions;
using WinGetStudio.Services.Settings;
using WinGetStudio.Services.Settings.Extensions;
using WinGetStudio.Services.Telemetry.Extensions;
using WinGetStudio.Services.VisualFeedback.Extensions;
using WinGetStudio.Services.WindowsPackageManager.Extensions;
using WinGetStudio.ViewModels;
using WinGetStudio.ViewModels.ConfigurationFlow;
using WinGetStudio.ViewModels.Controls;
using WinGetStudio.Views;
using WinGetStudio.Views.ConfigurationFlow;

namespace WinGetStudio;

public partial class App : Application
{
    public IHost Host { get; }

    private readonly DispatcherQueue _dispatcherQueue;

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static UIElement? AppTitlebar { get; set; }

    public App()
    {
        InitializeComponent();

        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        Host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .UseDefaultServiceProvider((context, options) =>
            {
                options.ValidateOnBuild = true;
            })
            .ConfigureServices((context, services) =>
            {
                // Default Activation Handler
                services.AddTransient<ActivationHandler<Windows.ApplicationModel.Activation.LaunchActivatedEventArgs>, DefaultActivationHandler>();

                // Other Activation Handlers
                services.AddTransient<IActivationHandler, ConfigurationFileActivationHandler>();
                services.AddTransient<IActivationHandler, FileActivationHandler>();

                // Services
                services.AddSingleton<IThemeApplierService, ThemeApplierService>();
                services.AddTransient<INavigationViewService, NavigationViewService>();
                services.AddTransient<IStringResource, StringResource>();
                services.AddSingleton<IActivationService, ActivationService>();
                services.AddSingleton<IAppPageService, AppPageService>();
                services.AddSingleton<IConfigurationPageService, ConfigurationPageService>();
                services.AddSingleton<IValidationPageService, ValidationPageService>();
                services.AddSingleton<IAppNavigationService, AppNavigationService>();
                services.AddSingleton<IConfigurationNavigationService, ConfigurationNavigationService>();
                services.AddSingleton<IValidationNavigationService, ValidationNavigationService>();
                services.AddSingleton<IAppInfoService, AppInfoService>();

                // Dispatcher Queue
                services.AddSingleton(_dispatcherQueue);
                services.AddSingleton<IUIDispatcher, UIDispatcher>();

                // Settings
                services.AddSingleton<IAppSettingsService, AppSettingsService>();
                services.AddSingleton<IFeatureSettingsService, ThemeFeatureSettings>();
                services.AddSingleton<IFeatureSettingsService, TelemetryFeatureSettings>();

                // Core Services
                services.AddCore();
                services.AddDSC();
                services.AddWinGet();
                services.AddSettings();
                services.AddTelemetry();
                services.AddVisualFeedback();

                // Views and ViewModels
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<SettingsPage>();
                services.AddTransient<ConfigurationViewModel>();
                services.AddTransient<ConfigurationPage>();
                services.AddTransient<ValidationViewModel>();
                services.AddTransient<ValidationPage>();
                services.AddTransient<ValidationFrameViewModel>();
                services.AddTransient<ValidationFramePage>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<MainPage>();
                services.AddTransient<ShellPage>();
                services.AddTransient<ShellViewModel>();
                services.AddTransient<PreviewFilePage>();
                services.AddTransient<PreviewFileViewModel>();
                services.AddTransient<ApplyFilePage>();
                services.AddTransient<ApplyFileViewModel>();
                services.AddTransient<NotificationPaneViewModel>();
                services.AddTransient<LoadingProgressBarViewModel>();

                // Factories
                services.AddSingleton<ValidationViewModelFactory>(sp => () => ActivatorUtilities.CreateInstance<ValidationViewModel>(sp));

                // Configuration
                services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
            })
            .UseLogger()
            .Build();

        UnhandledException += App_UnhandledException;
        AppInstance.GetCurrent().Activated += OnActivated;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    protected async override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        await App.GetService<IActivationService>().ActivateAsync(AppInstance.GetCurrent().GetActivatedEventArgs().Data);

        if (!await App.GetService<IDSC>().IsUnstubbedAsync())
        {
            await App.GetService<IDSC>().UnstubAsync();
        }
    }

    private async void OnActivated(object? sender, AppActivationArguments args)
    {
        var localArgsDataReference = args.Data;

        await _dispatcherQueue.EnqueueAsync(async () =>
        {
            await GetService<IActivationService>().ActivateAsync(localArgsDataReference);
        });
    }
}
