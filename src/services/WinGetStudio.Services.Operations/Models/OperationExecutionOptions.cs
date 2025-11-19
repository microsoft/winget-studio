// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using WinGetStudio.Services.Operations.Contracts;

namespace WinGetStudio.Services.Operations.Models;

public sealed record class OperationExecutionOptions
{
    public bool NotifyOnCompletion { get; init; }

    /// <summary>
    /// Gets the operation policies.
    /// </summary>
    public IReadOnlyList<IOperationPolicy> Policies { get; init; } = [];
}
