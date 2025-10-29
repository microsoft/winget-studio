// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Windows.Foundation.Collections;
using WinGetStudio.Services.DesiredStateConfiguration.Helpers;

namespace WinGetStudio.Services.DesiredStateConfiguration.Models;

public sealed partial class DSCPropertySet : Dictionary<string, object>
{
    public DSCPropertySet()
        : this(null)
    {
    }

    public DSCPropertySet(ValueSet valueSet = null)
        : base(valueSet ?? [])
    {
    }

    /// <summary>
    /// Deep copy the property set.
    /// </summary>
    /// <returns>A deep copy of the property set.</returns>
    public DSCPropertySet DeepCopy() => DSCYamlHelper.DeepCopy(this);

    /// <summary>
    /// Converts the current object to its YAML representation.
    /// </summary>
    /// <returns>A string containing the YAML representation of the current object.</returns>
    public string ToYaml() => DSCYamlHelper.ToYaml(this);

    /// <summary>
    /// Creates an instance of DSCPropertySet from a YAML string.
    /// </summary>
    /// <param name="input">The YAML string representation of a DSCPropertySet.</param>
    /// <returns>>An instance of DSCPropertySet.</returns>
    public static DSCPropertySet FromYaml(string input) => DSCYamlHelper.FromYaml<DSCPropertySet>(input);
}
