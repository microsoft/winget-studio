// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.Tracing;
using Microsoft.Diagnostics.Telemetry.Internal;
using WinGetStudio.Services.Telemetry.Models;

namespace WinGetStudio.Events;

[EventData]
internal sealed class NavigatedToPageEvent : EventBase
{
    public string PageName { get; set; }

    public NavigatedToPageEvent(string pageName)
    {
        PageName = pageName;
    }

    public override PartA_PrivTags PartA_PrivTags => PartA_PrivTags.ProductAndServiceUsage;
}
