// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WingetStudio.Services.VisualFeedback.Models;

internal sealed class LoadingChange
{
    /// <summary>
    /// Gets or sets a value indicating whether the loading indicator is visible.
    /// </summary>
    public bool IsVisible { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the loading indicator is indeterminate.
    /// </summary>
    public bool IsIndeterminate { get; set; }

    /// <summary>
    /// Gets or sets the progress value of the loading indicator.
    /// </summary>
    public int ProgressValue { get; set; }
}
