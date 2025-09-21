// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

public class DSCResource
{
    public string Name { get; set; }

    public Dictionary<string, DSCProperty> Properties { get; set; } = [];
}
