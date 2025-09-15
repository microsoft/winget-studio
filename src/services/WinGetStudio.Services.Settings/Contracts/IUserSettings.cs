// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using WinGetStudio.Services.Settings.Models;

namespace WinGetStudio.Services.Settings.Contracts;

public interface IUserSettings
{
    /// <summary>
    /// Occurs when the general settings are updated.
    /// </summary>
    /// <remarks>This event is triggered whenever the general settings are
    /// modified.  Subscribers can use this event to respond to changes in the
    /// settings.</remarks>
    event EventHandler<GeneralSettings> SettingsChanged;

    /// <summary>
    /// Gets the current general settings.
    /// </summary>
    GeneralSettings Current { get; }

    /// <summary>
    /// Gets the full path of the file or directory.
    /// </summary>
    string FullPath { get; }

    /// <summary>
    /// Initializes the user settings.
    /// </summary>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    Task InitializeAsync();

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

    /// <summary>
    /// Saves the specified general settings.
    /// </summary>
    /// <param name="newSettings">The new general settings to save.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    Task SaveAsync(GeneralSettings newSettings);
}
