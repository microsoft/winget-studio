// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

namespace WinGetStudio.Services.DesiredStateConfiguration.Models;
public class EditableDSCSet : IDSCSet
{
    public Guid InstanceIdentifier { get; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public IReadOnlyList<IDSCUnit> Units => InternalUnits.AsReadOnly();

    public List<IDSCUnit> InternalUnits = new();

    public void AddUnit(IDSCUnit unit)
    {
        InternalUnits.Add(unit);
    }

}
