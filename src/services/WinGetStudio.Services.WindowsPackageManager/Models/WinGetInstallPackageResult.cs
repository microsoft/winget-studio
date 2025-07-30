// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Services.WindowsPackageManager.Contracts;

namespace WinGetStudio.Services.WindowsPackageManager.Models;

internal sealed class WinGetInstallPackageResult : IWinGetInstallPackageResult
{
    /// <inheritdoc />
    public bool RebootRequired { get; init; }

    /// <inheritdoc />
    public int ExtendedErrorCode { get; init; }
}
