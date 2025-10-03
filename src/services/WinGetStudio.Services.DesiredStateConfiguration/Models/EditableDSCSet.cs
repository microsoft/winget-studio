// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

namespace WinGetStudio.Services.DesiredStateConfiguration.Models;

public sealed partial class EditableDSCSet : IDSCSet
{
    private readonly List<IDSCUnit> _units = [];

    public Guid InstanceIdentifier { get; } = Guid.NewGuid();

    public string Name { get; set; }

    public IReadOnlyList<IDSCUnit> Units => _units.AsReadOnly();

    public void AddUnit(IDSCUnit unit) => _units.Add(unit);

    public void ClearUnits() => _units.Clear();

    public void RemoveUnit(IDSCUnit unit) => _units.Remove(unit);

    public void UpdateUnit(IDSCUnit unit)
    {
        var index = _units.FindIndex(u => u.InstanceId == unit.InstanceId);
        if (index >= 0)
        {
            _units[index] = unit;
        }
    }
}
