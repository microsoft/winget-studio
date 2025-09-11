// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace WingetStudio.Services.VisualFeedback.Contracts;

public interface ILoadingService
{
    /// <summary>
    /// Event raised when the state of the loading service changes.
    /// </summary>
    event EventHandler StateChanged;

    /// <summary>
    /// Gets a value indicating whether the loading indicator is visible.
    /// </summary>
    bool IsVisible { get; }

    /// <summary>
    /// Gets the current progress value (0-100).
    /// </summary>
    int ProgressValue { get; }

    /// <summary>
    /// Gets a value indicating whether the loading indicator is in indeterminate state.
    /// </summary>
    bool IsIndeterminate { get; }

    /// <summary>
    /// Sets the visibility of the loading indicator.
    /// </summary>
    /// <param name="isVisible">True to make visible, false to hide.</param>
    void SetVisibility(bool isVisible);

    /// <summary>
    /// Sets the progress value (0-100).
    /// </summary>
    /// <param name="value">Progress value between 0 and 100.</param>
    void SetProgressValue(int value);

    /// <summary>
    /// Sets whether the loading indicator is in indeterminate state.
    /// </summary>
    /// <param name="isIndeterminate">True for indeterminate, false for determinate.</param>
    void SetIndeterminate(bool isIndeterminate);
}
