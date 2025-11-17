// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using WinGetStudio.Services.Operations.Contracts;

namespace WinGetStudio.Services.Operations.Models;

public sealed partial class OperationExecutionOptions
{
    /// <summary>
    /// Gets the operation policies.
    /// </summary>
    public IReadOnlyList<IOperationPolicy> Policies { get; init; } = [];
}
