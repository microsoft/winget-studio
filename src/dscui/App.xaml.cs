// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using dscui.Activation;
using dscui.Contracts.Services;
using dscui.Core.Contracts.Services;
using dscui.Core.Services;
using dscui.Models;
using dscui.Services;
using dscui.ViewModels;
using dscui.ViewModels.ConfigurationFlow;
using dscui.Views;
using dscui.Views.ConfigurationFlow;
using DSCUI.Services.DesiredStateConfiguration.Extensions;
using DSCUI.Services.WindowsPackageManager.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

namespace dscui;

public partial class App : Application
{
    public IHost Host { get; }

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

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers

            // Services
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();
            services.AddTransient<IStringResource, StringResource>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IAppPageService, AppPageService>();
            services.AddSingleton<IConfigurationPageService, ConfigurationPageService>();
            services.AddSingleton<IAppNavigationService, AppNavigationService>();
            services.AddSingleton<IConfigurationNavigationService, ConfigurationNavigationService>();

            // Core Services
            services.AddSingleton<IFileService, FileService>();
            services.AddDSC();
            services.AddWinGet();

            // Views and ViewModels
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<ConfigurationViewModel>();
            services.AddTransient<ConfigurationPage>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainPage>();
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();
            services.AddTransient<SelectFilePage>();
            services.AddTransient<SelectFileViewModel>();
            services.AddTransient<PreviewFilePage>();
            services.AddTransient<PreviewFileViewModel>();
            services.AddTransient<ApplyFilePage>();
            services.AddTransient<ApplyFileViewModel>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        await App.GetService<IActivationService>().ActivateAsync(args);
    }
}
