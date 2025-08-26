// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

namespace WinGetStudio.Services.DesiredStateConfiguration.Models;

public class EditableDSCSet : IDSCSet
{
    public Guid InstanceIdentifier { get; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public IReadOnlyList<IDSCUnit> Units => InternalUnits.AsReadOnly();

    public List<IDSCUnit> InternalUnits { get; } = new();

    public void AddUnit(IDSCUnit unit)
    {
        InternalUnits.Add(unit);
    }
}
