// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace WinGetStudio.Services.Core.Helpers;

public static class RuntimeHelper
{
    // Unique log path for each app instance
    private static readonly string _instanceLogPath = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff", CultureInfo.InvariantCulture);
    private static readonly Lazy<bool> _isMSIX = new(DetermineIsMSIX);
    private const string WinGetStudio = nameof(WinGetStudio);

    public const string SettingsFile = "settings.json";
    private const string TempStateDir = "TempState";
    private const string LocalStateDir = "LocalState";
    private const string LogsDir = "Logs";
    private const string ModuleCatalogsDir = "ModuleCatalogs";
    private const string MonacoWebUserDataDir = "MonacoWebUserData";

    /// <summary>
    /// Gets a value indicating whether the app is running as an MSIX package.
    /// </summary>
    public static bool IsMSIX => _isMSIX.Value;

    /// <summary>
    /// Gets the settings folder path.
    /// </summary>
    /// <returns>The settings folder path.</returns>
    public static string GetSettingsDirectory() => GetLocalStatePath();

    /// <summary>
    /// Gets the settings file path.
    /// </summary>
    /// <returns>The settings file path.</returns>
    public static string GetSettingsFilePath() => Path.Combine(GetSettingsDirectory(), SettingsFile);

    /// <summary>
    /// Gets the application logs folder.
    /// </summary>
    /// <returns>The application logs folder.</returns>
    public static string GetAppLogsPath() => Path.Combine(GetTempStatePath(), LogsDir);

    /// <summary>
    /// Gets the application instance log path.
    /// </summary>
    /// <returns>The application instance log path.</returns>
    public static string GetAppInstanceLogPath() => Path.Combine(GetAppLogsPath(), _instanceLogPath);

    /// <summary>
    /// Gets the module catalog cache path.
    /// </summary>
    /// <returns>The module catalog cache path.</returns>
    public static string GetModuleCatalogCachePath() => Path.Combine(GetLocalStatePath(), ModuleCatalogsDir);

    /// <summary>
    /// Gets the Monaco WebView2 user data directory.
    /// </summary>
    /// <returns>The Monaco WebView2 user data directory.</returns>
    public static string GetMonacoWebUserDataDirectory() => Path.Combine(GetTempStatePath(), MonacoWebUserDataDir);

    /// <summary>
    /// Gets the application version.
    /// </summary>
    /// <returns>The application version.</returns>
    public static Version GetAppVersion()
    {
        if (IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;
            return new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }

        return Assembly.GetExecutingAssembly().GetName().Version!;
    }

    /// <summary>
    /// Gets the unpackaged application data folder path.
    /// </summary>
    /// <param name="paths">The additional paths to combine.</param>
    /// <returns>The unpackaged application data folder path.</returns>
    private static string GetUnpackagedPath(params string[] paths)
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), WinGetStudio, Path.Combine(paths));
    }

    /// <summary>
    /// Gets the local state path.
    /// </summary>
    /// <returns>The local state path.</returns>
    private static string GetLocalStatePath()
    {
        if (IsMSIX)
        {
            return ApplicationData.Current.LocalFolder.Path;
        }

        return GetUnpackagedPath(LocalStateDir);
    }

    /// <summary>
    /// Gets the temporary state path.
    /// </summary>
    /// <returns>The temporary state path.</returns>
    private static string GetTempStatePath()
    {
        if (IsMSIX)
        {
            return ApplicationData.Current.TemporaryFolder.Path;
        }

        return GetUnpackagedPath(TempStateDir);
    }

    /// <summary>
    /// Determines if the app is running as an MSIX package.
    /// </summary>
    /// <returns>True if the app is running as an MSIX package; otherwise, false.</returns>
    private static bool DetermineIsMSIX()
    {
        uint length = 0;
        var result = PInvoke.GetCurrentPackageFullName(ref length, null);
        return result != WIN32_ERROR.APPMODEL_ERROR_NO_PACKAGE;
    }
}
