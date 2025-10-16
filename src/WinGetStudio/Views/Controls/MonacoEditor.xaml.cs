// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Timers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Windows.System;
using Windows.UI;
using WinGetStudio.Services.Core.Helpers;

namespace WinGetStudio.Views.Controls;

public sealed partial class MonacoEditor : UserControl
{
    private const string HostName = "MonacoAssets";
    private const string WebView2UserDataFolderEnvVar = "WEBVIEW2_USER_DATA_FOLDER";
    private readonly JsonSerializerOptions _options;

    // Throttle for TextChanged event
    private readonly DispatcherTimer _timer;
    private bool _pending;

    /// <summary>
    /// Raised when the text in the editor changes.
    /// </summary>
    public event EventHandler? TextChanged;

    // APIs
    private const string GetTextApi = "getText";
    private const string SetTextApi = "setText";
    private const string SetThemeApi = "setTheme";
    private const string SetLanguageApi = "setLanguage";
    private const string ContentChangedApi = "contentChanged";

    /// <summary>
    /// Gets the path to the Monaco assets.
    /// </summary>
    private string MonacoAssetsPath => Path.Combine(AppContext.BaseDirectory, "Assets", "Monaco");

    public string? Text { get; private set; }

    public MonacoEditor()
    {
        _timer = new() { Interval = TimeSpan.FromMilliseconds(250) };
        _timer.Tick += OnTimerTick;
        _options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        InitializeComponent();
        SetIsLoading(true);
        Environment.SetEnvironmentVariable(WebView2UserDataFolderEnvVar, RuntimeHelper.GetMonacoWebUserDataDirectory(), EnvironmentVariableTarget.Process);
    }

    /// <summary>
    /// Get the current text from the editor asynchronously.
    /// </summary>
    /// <returns>The current text from the editor.</returns>
    public async Task<string?> GetTextAsync()
    {
        var msg = new EditorMessage() { Type = GetTextApi };
        var response = await PostAndWaitForResponseAsync(msg);
        return response?.Value;
    }

    /// <summary>
    /// Set the text in the editor.
    /// </summary>
    /// <param name="text">The text to set.</param>
    public void SetText(string? text)
    {
        Text = text;
        var msg = new EditorMessage() { Type = SetTextApi, Value = text };
        var json = JsonSerializer.Serialize(msg, _options);
        Editor.CoreWebView2.PostWebMessageAsJson(json);
    }

    /// <summary>
    /// Update the editor theme based on the current application theme.
    /// </summary>
    public void UpdateTheme()
    {
        var theme = Application.Current.RequestedTheme == ApplicationTheme.Light ? "vs" : "vs-dark";
        var msg = new EditorMessage() { Type = SetThemeApi, Value = theme };
        var json = JsonSerializer.Serialize(msg, _options);
        Editor.CoreWebView2.PostWebMessageAsJson(json);
    }

    /// <summary>
    /// Set the language of the editor.
    /// </summary>
    /// <param name="language">The language to set (e.g., "json", "yaml").</param>
    public void SetLanguage(string language)
    {
        var msg = new EditorMessage() { Type = SetLanguageApi, Value = language };
        var json = JsonSerializer.Serialize(msg, _options);
        Editor.CoreWebView2.PostWebMessageAsJson(json);
    }

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
        UpdateTheme();
        SetIsLoading(false);
        SetText(Text);
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
            var message = JsonSerializer.Deserialize<EditorMessage>(args.WebMessageAsJson);
            if (message?.Type == ContentChangedApi)
            {
                OnContentChanged(message.Value);
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
    private void OnContentChanged(string? text)
    {
        Text = text;

        // If the timer is running, mark pending change and return
        if (_timer.IsEnabled)
        {
            _pending = true;
            return;
        }

        // Timer is not running, raise event and start timer
        TextChanged?.Invoke(this, EventArgs.Empty);
        _timer.Start();
    }

    /// <summary>
    /// Handle the elapsed event of the text changed throttle timer.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private void OnTimerTick(object? sender, object e)
    {
        // If there are pending changes, raise event and reset pending flag
        if (_pending)
        {
            _pending = false;
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
    /// Show a dialog to open a URI.
    /// </summary>
    /// <param name="uri">The URI to open.</param>
    private async Task ShowOpenUriDialogAsync(Uri uri)
    {
        OpenUriDialog.Content = uri.ToString();
        var result = await OpenUriDialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await Launcher.LaunchUriAsync(uri);
        }
    }

    /// <summary>
    /// Post a message to the editor and wait for a response.
    /// </summary>
    /// <param name="postMessage">The message to post.</param>
    /// <returns>The response message, or null if no response is received.</returns>
    private async Task<EditorMessage?> PostAndWaitForResponseAsync(EditorMessage postMessage)
    {
        var tcs = new TaskCompletionSource<EditorMessage?>();
        void Handler(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var receivedMessage = JsonSerializer.Deserialize<EditorMessage>(e.WebMessageAsJson);
                if (receivedMessage?.Type == postMessage.Type)
                {
                    tcs.TrySetResult(receivedMessage);
                }
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }

        try
        {
            Editor.CoreWebView2.WebMessageReceived += Handler;
            var json = JsonSerializer.Serialize(postMessage);
            Editor.CoreWebView2.PostWebMessageAsJson(json);
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            using (cts.Token.Register(() => tcs.TrySetCanceled()))
            {
                return await tcs.Task;
            }
        }
        catch
        {
            return null;
        }
        finally
        {
            Editor.CoreWebView2.WebMessageReceived -= Handler;
        }
    }

    /// <summary>
    /// Represents a message from the web content to the host application.
    /// </summary>
    private sealed partial class EditorMessage
    {
        public const string TypePropertyName = "type";
        public const string ValuePropertyName = "value";

        [JsonPropertyName(TypePropertyName)]
        public string? Type { get; set; }

        [JsonPropertyName(ValuePropertyName)]
        public string? Value { get; set; }
    }
}
