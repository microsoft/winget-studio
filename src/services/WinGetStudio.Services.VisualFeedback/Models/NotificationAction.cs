// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace WingetStudio.Services.VisualFeedback.Models;

public sealed class NotificationAction
{
    public string Label { get; set; }

    public Action Callback { get; set; }
}
