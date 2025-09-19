// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;

public interface IDSCExplorer
{
    Task<IReadOnlyList<IDSCModule>> GetDSCModulesAsync();

    Task<IReadOnlyList<IDSCModule>> GetDSCModulesAsync<TModuleProvider>()
        where TModuleProvider : IModuleProvider;
}
