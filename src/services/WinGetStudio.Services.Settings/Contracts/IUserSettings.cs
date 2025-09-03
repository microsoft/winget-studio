// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using WinGetStudio.Services.Settings.Models;

namespace WinGetStudio.Services.Settings.Contracts;

public interface IUserSettings
{
    event EventHandler<IGeneralSettings> SettingsChanged;

    IGeneralSettings Current { get; }

    string FullPath { get; }

    /// <summary>
    /// Applies the specified changes to the general settings and saves them.
    /// </summary>
    /// <remarks>The changes are applied to the current general settings, and
    /// the updated settings are persisted.  Ensure the delegate modifies the
    /// settings as intended, as the changes will be saved
    /// immediately.</remarks>
    /// <param name="changes">A delegate that defines the modifications to apply to the general settings.
    /// The provided <see cref="GeneralSettings"/> instance is preloaded with the current settings.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    Task SaveAsync(Action<GeneralSettings> changes);
}
