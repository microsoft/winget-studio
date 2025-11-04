// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.System.Profile;
using WinGetStudio.Contracts.Views;
using WinGetStudio.ViewModels;

namespace WinGetStudio.Views;

public sealed partial class SettingsPage : Page, IView<SettingsViewModel>
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
    }

    private void CopyVersionToClipboard(object? sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(ViewModel.VersionDescription))
        {
            var dataPackage = new DataPackage();

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine(ViewModel.VersionDescription);

                // Try to get the OS build and revision from the DeviceFamilyVersion
                var deviceFamilyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
                if (!string.IsNullOrEmpty(deviceFamilyVersion) && ulong.TryParse(deviceFamilyVersion, out var v))
                {
                    var build = (v >> 16) & 0xffff;
                    var revision = v & 0xffff;
                    sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "OS Build {0}.{1}", build, revision));
                }
                else
                {
                    // Fallback to Environment.OSVersion if parsing fails
                    try
                    {
                        var osVer = Environment.OSVersion.Version;
                        sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "OS Build: {0}.{1}", osVer.Build, osVer.Revision));
                    }
                    catch
                    {
                        // If even that fails, don't block copying the rest
                    }
                }

                // OS architecture
                var arch = RuntimeInformation.OSArchitecture;
                var archStr = arch switch
                {
                    Architecture.X64 => "x64",
                    Architecture.X86 => "x86",
                    Architecture.Arm64 => "arm64",
                    Architecture.Arm => "arm",
                    _ => arch.ToString().ToLowerInvariant(),
                };

                sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "Architecture: {0}", archStr));

                dataPackage.SetText(sb.ToString());
                Clipboard.SetContent(dataPackage);
            }
            catch
            {
                // On any unexpected error, fall back to copying just the version description
                dataPackage.SetText(ViewModel.VersionDescription);
                Clipboard.SetContent(dataPackage);
            }
        }
    }
}
