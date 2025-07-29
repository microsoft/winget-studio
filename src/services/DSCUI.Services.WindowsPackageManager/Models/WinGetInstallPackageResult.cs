// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using DSCUI.Services.WindowsPackageManager.Contracts;

namespace DSCUI.Services.WindowsPackageManager.Models;

internal sealed class WinGetInstallPackageResult : IWinGetInstallPackageResult
{
    /// <inheritdoc />
    public bool RebootRequired { get; init; }

    /// <inheritdoc />
    public int ExtendedErrorCode { get; init; }
}
