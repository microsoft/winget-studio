// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace WinGetStudio.Services.DesiredStateConfiguration.Contracts;

public interface IDSCFactory
{
    public IDSCUnit CreateUnit(IDSCUnit unit);

    public Task<IDSCSet> CreateSetAsync(IDSCSet dscSet);
}
