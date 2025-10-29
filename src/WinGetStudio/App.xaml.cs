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
using WinGetStudio.Services;
using WinGetStudio.Services.Core.Extensions;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Extensions;
using WinGetStudio.Services.DesiredStateConfiguration.Extensions;
using WingetStudio.Services.Localization.Extensions;
using WinGetStudio.Services.Logging.Extensions;
using WinGetStudio.Services.Navigation;
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
    private const string AppSettingsFileName = "appsettings.json";

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
                services.AddTransient<IActivationHandler, FileActivationHandler>();

                // Services
                services.AddSingleton<IThemeApplierService, ThemeApplierService>();
                services.AddTransient<IAppShellNavigationViewService, AppShellNavigationViewService>();
                services.AddSingleton<IActivationService, ActivationService>();
                services.AddSingleton<IAppPageService, AppPageService>();
                services.AddSingleton<IConfigurationPageService, ConfigurationPageService>();
                services.AddSingleton<IAppFrameNavigationService, AppFrameNavigationService>();
                services.AddSingleton<IConfigurationFrameNavigationService, ConfigurationFrameNavigationService>();
                services.AddSingleton<IAppInfoService, AppInfoService>();
                services.AddSingleton<IConfigurationManager, ConfigurationManager>();

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
                services.AddDSCExplorer();
                services.AddWinGet();
                services.AddSettings();
                services.AddTelemetry();
                services.AddVisualFeedback();
                services.AddLogging(AppSettingsFileName);
                services.AddReswLocalization();

                // Views and ViewModels
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<SettingsPage>();
                services.AddTransient<ConfigurationViewModel>();
                services.AddTransient<ConfigurationPage>();
                services.AddTransient<ValidationViewModel>();
                services.AddTransient<ValidationPage>();
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
                services.AddTransient<ResourceAutoSuggestBoxViewModel>();

                // Factories
                services.AddTransient<ValidationViewModelFactory>(sp => () => ActivatorUtilities.CreateInstance<ValidationViewModel>(sp));
                services.AddTransient<ApplySetViewModelFactory>(sp => applySet => ActivatorUtilities.CreateInstance<ApplySetViewModel>(sp, applySet));
                services.AddTransient<ResourceExplorerViewModelFactory>(sp => resource => ActivatorUtilities.CreateInstance<ResourceExplorerViewModel>(sp, resource));
            })
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
