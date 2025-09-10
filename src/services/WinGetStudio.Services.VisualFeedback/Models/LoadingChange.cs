// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WingetStudio.Services.VisualFeedback.Models;

internal sealed class LoadingChange
{
    public bool IsVisible { get; set; }

    public bool IsIndeterminate { get; set; }

    public int ProgressValue { get; set; }
}
