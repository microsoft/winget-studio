// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;

internal interface IDSCResourceJsonSchemaDefaultGenerator
{
    Task<string> GenerateDefaultYamlFromSchemaAsync(string jsonSchema);

    Task<string> GenerateDefaultJsonFromSchemaAsync(string jsonSchema);
}
