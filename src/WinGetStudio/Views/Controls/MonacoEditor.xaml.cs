// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Windows.System;
using Windows.UI;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Services.Core.Helpers;
using WinGetStudio.Services.Settings;

namespace WinGetStudio.Views.Controls;

public sealed partial class MonacoEditor : UserControl
{
    private const string SetTextApi = "setText";
    private const string SetThemeApi = "setTheme";
    private const string SetLanguageApi = "setLanguage";
    private const string SetReadOnlyApi = "setReadOnly";
    private const string ContentChangedApi = "contentChanged";

    private const string LightTheme = "vs";
    private const string DarkTheme = "vs-dark";

    private const string HostName = "MonacoAssets";
    private const string WebView2UserDataFolderEnvVar = "WEBVIEW2_USER_DATA_FOLDER";

    private const int InitialThrottleInterval = 500;

    private readonly ThemeFeatureSettings _themeSettings;
    private readonly JsonSerializerOptions _options;
    private readonly DispatcherTimer _timer;

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(MonacoEditor), new PropertyMetadata(null, OnTextPropertyChanged));
    public static readonly DependencyProperty SyntaxProperty = DependencyProperty.Register(nameof(Syntax), typeof(string), typeof(MonacoEditor), new PropertyMetadata(null, OnSyntaxPropertyChanged));
    public static readonly DependencyProperty UseThrottleProperty = DependencyProperty.Register(nameof(UseThrottle), typeof(bool), typeof(MonacoEditor), new PropertyMetadata(true));
    public static readonly DependencyProperty ThrottleIntervalProperty = DependencyProperty.Register(nameof(ThrottleInterval), typeof(int), typeof(MonacoEditor), new PropertyMetadata(InitialThrottleInterval, OnThrottleIntervalPropertyChanged));
    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(MonacoEditor), new PropertyMetadata(false, OnIsReadOnlyPropertyChanged));

    private bool _pending;
    private string? _unboundText;
    private bool _internalSet;

    /// <summary>
    /// Raised when the text in the editor changes.
    /// </summary>
    public event EventHandler? TextChanged;

    /// <summary>
    /// Gets the path to the Monaco assets.
    /// </summary>
    private string MonacoAssetsPath => Path.Combine(AppContext.BaseDirectory, "Assets", "Monaco");

    /// <summary>
    /// Gets or sets the text in the editor.
    /// </summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets the syntax/language of the editor (e.g., "json", "yaml").
    /// </summary>
    public string? Syntax
    {
        get => (string?)GetValue(SyntaxProperty);
        set => SetValue(SyntaxProperty, value);
    }

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether to use throttling for text change events.
    /// </summary>
    public bool UseThrottle
    {
        get => (bool)GetValue(UseThrottleProperty);
        set => SetValue(UseThrottleProperty, value);
    }

    /// <summary>
    /// Gets or sets the throttle interval in milliseconds.
    /// </summary>
    public int ThrottleInterval
    {
        get => (int)GetValue(ThrottleIntervalProperty);
        set => SetValue(ThrottleIntervalProperty, value);
    }

    public MonacoEditor()
    {
        _timer = new();
        _timer.Tick += OnTimerTick;
        _options = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

        var appSettings = App.GetService<IAppSettingsService>();
        _themeSettings = appSettings.GetFeature<ThemeFeatureSettings>();

        InitializeComponent();
        SetIsLoading(true);
        SetThrottleIntervalInternal(ThrottleInterval);
        Environment.SetEnvironmentVariable(WebView2UserDataFolderEnvVar, RuntimeHelper.GetMonacoWebUserDataDirectory(), EnvironmentVariableTarget.Process);
    }

    /// <summary>
    /// Handle changes to the Text dependency property.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private static void OnTextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var control = (MonacoEditor)sender;

        // Only update the editor if the change is external
        if (!control._internalSet)
        {
            control._unboundText = e.NewValue?.ToString();
            control.SetEditorText(control._unboundText);
        }
    }

    /// <summary>
    /// Handle changes to the Syntax dependency property.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private static void OnSyntaxPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var control = (MonacoEditor)sender;
        control.SetLanguage(e.NewValue?.ToString());
    }

    /// <summary>
    /// Handle changes to the IsReadOnly dependency property.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private static void OnIsReadOnlyPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var control = (MonacoEditor)sender;
        control.SetReadOnly((bool)e.NewValue);
    }

    /// <summary>
    /// Handle changes to the ThrottleInterval dependency property.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private static void OnThrottleIntervalPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var control = (MonacoEditor)sender;
        var newInterval = e.NewValue is int interval && interval > 0 ? interval : InitialThrottleInterval;
        control._timer.Interval = TimeSpan.FromMilliseconds(newInterval);
    }

    /// <summary>
    /// Set the text in the editor internally.
    /// </summary>
    /// <param name="text">The text to set.</param>
    private void SetTextPropertyInternal(string? text)
    {
        _internalSet = true;
        Text = text;
        _internalSet = false;
    }

    /// <summary>
    /// Handle theme changes from the theme applier service.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="newTheme">The new theme.</param>
    private void OnThemeApplied(object? sender, ElementTheme newTheme)
    {
        DispatcherQueue.TryEnqueue(() => SetTheme(newTheme));
    }

    /// <summary>
    /// Update the editor theme based on the current application theme.
    /// </summary>
    private void SetTheme(ElementTheme newTheme)
    {
        if (Editor.CoreWebView2 != null)
        {
            // Resolve the theme to use
            var theme = newTheme switch
            {
                ElementTheme.Light => LightTheme,
                ElementTheme.Dark => DarkTheme,
                _ => _themeSettings.DefaultAppTheme == ApplicationTheme.Light ? LightTheme : DarkTheme,
            };

            // Send the theme change message to the editor
            var msg = new EditorMessage<string>() { Type = SetThemeApi, Value = theme };
            var json = JsonSerializer.Serialize(msg, _options);
            Editor.CoreWebView2.PostWebMessageAsJson(json);
        }
    }

    /// <summary>
    /// Set the language of the editor.
    /// </summary>
    /// <param name="language">The language to set (e.g., "json", "yaml").</param>
    private void SetLanguage(string? language)
    {
        if (Editor.CoreWebView2 != null)
        {
            var msg = new EditorMessage<string>() { Type = SetLanguageApi, Value = language };
            var json = JsonSerializer.Serialize(msg, _options);
            Editor.CoreWebView2.PostWebMessageAsJson(json);
        }
    }

    /// <summary>
    /// Set the read-only state of the editor.
    /// </summary>
    /// <param name="isReadOnly">True to set read-only, false otherwise.</param>
    private void SetReadOnly(bool isReadOnly)
    {
        if (Editor.CoreWebView2 != null)
        {
            var msg = new EditorMessage<bool>() { Type = SetReadOnlyApi, Value = isReadOnly };
            var json = JsonSerializer.Serialize(msg, _options);
            Editor.CoreWebView2.PostWebMessageAsJson(json);
        }
    }

    /// <summary>
    /// Handle the Loaded event of the WebView2 control.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private async void Editor_Loaded(object sender, RoutedEventArgs e)
    {
        // Initialize WebView2
        await Editor.EnsureCoreWebView2Async();
        Editor.CoreWebView2.SetVirtualHostNameToFolderMapping(HostName, MonacoAssetsPath, CoreWebView2HostResourceAccessKind.Allow);

        // Set transparent background
        Editor.DefaultBackgroundColor = Color.FromArgb(0, 0, 0, 0);

        // Handle WebView2 events
        Editor.CoreWebView2.NavigationCompleted += OnNavigationCompleted;
        Editor.CoreWebView2.PermissionRequested += OnPermissionRequested;
        Editor.CoreWebView2.NewWindowRequested += OnNewWindowRequested;

        // Configure WebView2 settings
        var editorSettings = Editor.CoreWebView2.Settings;
        editorSettings.AreDefaultScriptDialogsEnabled = false;
        editorSettings.AreDefaultContextMenusEnabled = false;
        editorSettings.AreHostObjectsAllowed = false;
        editorSettings.IsGeneralAutofillEnabled = false;
        editorSettings.IsPasswordAutosaveEnabled = false;
        editorSettings.IsZoomControlEnabled = false;
        editorSettings.IsStatusBarEnabled = false;
        editorSettings.AreDevToolsEnabled = false;
        DebugModeSettings(editorSettings);

        // Load the Monaco Editor HTML page
        Editor.CoreWebView2.Navigate($"https://{HostName}/editor.html");

        // Subscribe to theme changes
        _themeSettings.ThemeApplied += OnThemeApplied;
    }

    /// <summary>
    /// Handle the Unloaded event of the WebView2 control.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private void Editor_Unloaded(object sender, RoutedEventArgs e)
    {
        // Unsubscribe from theme changes
        _themeSettings.ThemeApplied -= OnThemeApplied;
    }

    /// <summary>
    /// Set WebView2 settings for debug mode only.
    /// </summary>
    /// <param name="settings">The WebView2 settings.</param>
    [Conditional("DEBUG")]
    private static void DebugModeSettings(CoreWebView2Settings settings)
    {
        settings.AreDevToolsEnabled = true;
    }

    /// <summary>
    /// Handle navigation completed event.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The event args.</param>
    private void OnNavigationCompleted(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        SetTheme(_themeSettings.Theme);
        SetLanguage(Syntax);
        SetReadOnly(IsReadOnly);
        SetIsLoading(false);
        SetEditorText(Text);
        Editor.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
        Editor.Focus(FocusState.Programmatic);
    }

    /// <summary>
    /// Handle permission requested event.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The event args.</param>
    private void OnPermissionRequested(CoreWebView2 sender, CoreWebView2PermissionRequestedEventArgs args)
    {
        // Automatically allow clipboard read permissions
        if (args.PermissionKind == CoreWebView2PermissionKind.ClipboardRead)
        {
            args.State = CoreWebView2PermissionState.Allow;
            args.Handled = true;
        }
    }

    /// <summary>
    /// Handle new window requested event.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The event args.</param>
    private async void OnNewWindowRequested(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
    {
        // Intercept new window requests to handle external links
        if (args.Uri != null && args.IsUserInitiated)
        {
            args.Handled = true;
            await ShowOpenUriDialogAsync(new Uri(args.Uri));
        }
    }

    /// <summary>
    /// Handle web message received event.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The event args.</param>
    private void OnWebMessageReceived(CoreWebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        try
        {
            var message = JsonSerializer.Deserialize<EditorMessage<string>>(args.WebMessageAsJson);
            if (message?.Type == ContentChangedApi)
            {
                OnEditorContentChanged(message.Value);
            }
        }
        catch
        {
            // No-op
        }
    }

    /// <summary>
    /// Handle the content changed event from the editor.
    /// </summary>
    /// <param name="text">The new text content.</param>
    private void OnEditorContentChanged(string? text)
    {
        // Always update the unbound text
        _unboundText = text;

        // If the timer is running, mark pending change and return
        if (_timer.IsEnabled)
        {
            _pending = true;
            return;
        }

        // Set the text property internally
        SetTextPropertyInternal(text);

        // Timer is not running, raise event and start timer
        TextChanged?.Invoke(this, EventArgs.Empty);

        // If throttling is enabled, start the timer otherwise, don't start the
        // timer and allow immediate updates
        if (UseThrottle)
        {
            _timer.Start();
        }
    }

    /// <summary>
    /// Set the text in the editor.
    /// </summary>
    /// <param name="text">The text to set.</param>
    private void SetEditorText(string? text)
    {
        if (Editor.CoreWebView2 != null)
        {
            var msg = new EditorMessage<string>() { Type = SetTextApi, Value = text };
            var json = JsonSerializer.Serialize(msg, _options);
            Editor.CoreWebView2.PostWebMessageAsJson(json);
        }
    }

    /// <summary>
    /// Handle the elapsed event of the text changed throttle timer.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private void OnTimerTick(object? sender, object e)
    {
        // If there are pending changes, update the text property and raise event
        if (_pending)
        {
            _pending = false;
            SetTextPropertyInternal(_unboundText);
            TextChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        // No pending changes, stop the timer
        _timer.Stop();
    }

    /// <summary>
    /// Set the loading state of the editor.
    /// </summary>
    /// <param name="isLoading">True if loading, false otherwise.</param>
    private void SetIsLoading(bool isLoading)
    {
        LoadingProgressRing.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        Editor.Visibility = isLoading ? Visibility.Collapsed : Visibility.Visible;
    }

    /// <summary>
    /// Sets the throttle interval internally.
    /// </summary>
    /// <param name="interval">The interval in milliseconds.</param>
    private void SetThrottleIntervalInternal(int interval) => _timer.Interval = TimeSpan.FromMilliseconds(interval);

    /// <summary>
    /// Show a dialog to open a URI.
    /// </summary>
    /// <param name="uri">The URI to open.</param>
    private async Task ShowOpenUriDialogAsync(Uri uri)
    {
        try
        {
            OpenUriDialog.Content = uri.ToString();
            var result = await OpenUriDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await Launcher.LaunchUriAsync(uri);
            }
        }
        catch
        {
            // No-op
        }
    }

    /// <summary>
    /// Represents a message from the web content to the host application.
    /// </summary>
    private sealed partial class EditorMessage<T>
    {
        public const string TypePropertyName = "type";
        public const string ValuePropertyName = "value";

        [JsonPropertyName(TypePropertyName)]
        public string? Type { get; set; }

        [JsonPropertyName(ValuePropertyName)]
        public T? Value { get; set; }
    }
}
