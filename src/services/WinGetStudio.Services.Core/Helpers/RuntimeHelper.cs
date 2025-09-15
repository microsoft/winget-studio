// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Windows.ApplicationModel;
using Windows.Storage;

namespace WinGetStudio.Services.Core.Helpers;

public static class RuntimeHelper
{
    // Unique log path for each app instance
    private static readonly string _instanceLogPath = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff", CultureInfo.InvariantCulture);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetCurrentPackageFullName(ref int packageFullNameLength, StringBuilder packageFullName);

    /// <summary>
    /// Gets a value indicating whether the app is running as an MSIX package.
    /// </summary>
    public static bool IsMSIX
    {
        get
        {
            var length = 0;
            return GetCurrentPackageFullName(ref length, null) != 15700L;
        }
    }

    /// <summary>
    /// Gets the application version.
    /// </summary>
    /// <returns>The application version.</returns>
    public static Version GetAppVersion()
    {
        Version version;
        if (IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;
            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return version;
    }

    /// <summary>
    /// Gets the application logs folder.
    /// </summary>
    /// <returns>The application logs folder.</returns>
    public static string GetAppLogsPath()
    {
        return Path.Combine(ApplicationData.Current.TemporaryFolder.Path, "Logs");
    }

    /// <summary>
    /// Gets the application instance log path.
    /// </summary>
    /// <returns>The application instance log path.</returns>
    public static string GetAppInstanceLogPath()
    {
        return Path.Combine(GetAppLogsPath(), _instanceLogPath);
    }
}
