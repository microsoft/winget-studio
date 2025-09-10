// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace WingetStudio.Services.VisualFeedback.Contracts;

public interface ILoadingService
{
    event EventHandler StateChanged;

    bool IsVisible { get; }

    int ProgressValue { get; }

    bool IsIndeterminate { get; }

    void SetVisibility(bool isVisible);

    void SetProgressValue(int value);

    void SetIndeterminate(bool isIndeterminate);
}
