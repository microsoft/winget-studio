// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Windows.UI;

namespace WinGetStudio.Views.Controls;

public sealed partial class MonacoEditor : UserControl
{
    private readonly JsonSerializerOptions _options;
    private const string HostName = "MonacoAssets";

    private string MonacoAssetsPath => Path.Combine(AppContext.BaseDirectory, "Assets", "Monaco");

    public MonacoEditor()
    {
        _options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        InitializeComponent();
        SetIsLoading(true);
    }

    private async void Editor_Loaded(object sender, RoutedEventArgs e)
    {
        await Editor.EnsureCoreWebView2Async();
        Editor.DefaultBackgroundColor = Color.FromArgb(0, 0, 0, 0);
        Editor.CoreWebView2.SetVirtualHostNameToFolderMapping(HostName, MonacoAssetsPath, CoreWebView2HostResourceAccessKind.Allow);

        // Handle WebView2 events
        Editor.CoreWebView2.NavigationCompleted += OnNavigationCompleted;

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
        SetIsLoading(false);
        Editor.Focus(FocusState.Programmatic);
    }

    /// <summary>
    /// Set the loading state of the editor.
    /// </summary>
    /// <param name="isLoading">True if loading, false otherwise.</param>
    private void SetIsLoading(bool isLoading)
    {
        LoadingProgressRing.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// Get the current text from the editor asynchronously.
    /// </summary>
    /// <returns>The current text from the editor.</returns>
    public async Task<string?> GetTextAsync()
    {
        var tcs = new TaskCompletionSource<string?>();

        void Handler(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var doc = JsonDocument.Parse(e.WebMessageAsJson).RootElement;
                if (doc.TryGetProperty("type", out var typeProp) && typeProp.GetString() == "value")
                {
                    var text = doc.TryGetProperty("value", out var valProp) ? valProp.GetString() : null;
                    tcs.TrySetResult(text);
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
            var message = JsonSerializer.Serialize(new EditorMessage<object>("getValue"));
            Editor.CoreWebView2.PostWebMessageAsJson(message);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
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
    /// Set the text in the editor.
    /// </summary>
    /// <param name="text">The text to set.</param>
    public void SetText(string text)
    {
        var msg = new EditorMessage<string>("setValue", text);
        var json = JsonSerializer.Serialize(msg, _options);
        Editor.CoreWebView2.PostWebMessageAsJson(json);
    }

    /// <summary>
    /// Represents a message from the web content to the host application.
    /// </summary>
    private sealed record EditorMessage<T>(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("payload")] T? Payload = default);
}
