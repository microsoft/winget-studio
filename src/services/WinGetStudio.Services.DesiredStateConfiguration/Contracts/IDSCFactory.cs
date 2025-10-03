// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Management.Configuration;

namespace WinGetStudio.Services.DesiredStateConfiguration.Contracts;

internal interface IDSCFactory
{
    IDSCUnit CreateUnit(IDSCUnit unit);

    IDSCSet CreateSet(IDSCSet dscSet);

    /// <summary>
    /// Create a configuration processor using DSC configuration API
    /// </summary>
    /// <returns>Configuration processor</returns>
    Task<ConfigurationProcessor> CreateProcessorAsync();
}
