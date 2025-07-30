// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Contracts;
public interface IDSCFactory
{
    public IDSCUnit CreateUnit(IDSCUnit unit);
    public Task<IDSCSet> CreateSetAsync(EditableDSCSet set);
}
