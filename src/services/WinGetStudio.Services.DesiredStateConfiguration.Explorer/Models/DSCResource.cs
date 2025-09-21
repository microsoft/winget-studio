// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

public sealed class DSCResource
{
    public string Name { get; set; }

    public string Syntax { get; set; }

    public Dictionary<string, DSCProperty> Properties { get; set; } = [];
}
