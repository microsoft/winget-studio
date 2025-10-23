// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;

internal interface IDSCProcess
{
    Task<DSCProcessResult> GetResourceSchemaAsync(string resource);
}
