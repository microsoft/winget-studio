// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.Contracts.Models;

public interface ISessionStateAware<T>
{
    /// <summary>
    /// Captures the current state of the source object.
    /// </summary>
    /// <param name="source">The source object to capture state from.</param>
    void CaptureState(T source);

    /// <summary>
    /// Restores the state to the source object.
    /// </summary>
    /// <param name="source">The source object to restore state to.</param>
    void RestoreState(T source);

    /// <summary>
    /// Gets a value indicating whether the state can be restored.
    /// </summary>
    /// <returns>True if the state can be restored; otherwise, false.</returns>
    bool CanRestoreState();

    /// <summary>
    /// Clears the captured state.
    /// </summary>
    void ClearState();
}
