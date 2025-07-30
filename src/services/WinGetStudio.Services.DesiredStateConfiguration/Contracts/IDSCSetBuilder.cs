// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Contracts;
public interface IDSCSetBuilder
{
    public string FilePath { get; set; }

    public IReadOnlyList<IDSCUnit> Units { get; }
    public void AddUnit(EditableDSCUnit unit);
    public void RemoveUnit(EditableDSCUnit unit);
    public void ClearUnits();
    public void UpdateUnit(EditableDSCUnit unit);
    public void ImportSet(IDSCSet set);
    public bool IsEmpty();
    public Task<IDSCSet> BuildAsync();
    public Task<bool> EqualsYaml(string yaml);

    public Task<string> ConvertToYamlAsync();
}
