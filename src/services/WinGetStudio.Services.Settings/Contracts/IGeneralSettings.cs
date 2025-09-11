// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Services.Settings.Models;

namespace WinGetStudio.Services.Settings.Contracts;

public interface IGeneralSettings
{
    /// <summary>
    /// Gets the application theme.
    /// </summary>
    string Theme { get; }

    /// <summary>
    /// Clones the current settings instance.
    /// </summary>
    /// <returns>A new instance of <see cref="GeneralSettings"/> with the same property values.</returns>
    GeneralSettings Clone();
}
