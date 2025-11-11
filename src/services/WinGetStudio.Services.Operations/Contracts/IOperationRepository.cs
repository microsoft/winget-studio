// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using WinGetStudio.Services.Operations.Models;
using WinGetStudio.Services.Operations.Models.State;

namespace WinGetStudio.Services.Operations.Contracts;

internal interface IOperationRepository
{
    /// <summary>
    /// Gets the snapshots of all operations.
    /// </summary>
    IReadOnlyList<OperationSnapshot> Snapshots { get; }

    /// <summary>
    /// Gets all operations.
    /// </summary>
    IReadOnlyList<OperationContext> Operations { get; }

    /// <summary>
    /// Adds an operation.
    /// </summary>
    /// <param name="operation">The operation to add.</param>
    void Add(OperationContext operation);

    /// <summary>
    /// Removes an operation by ID.
    /// </summary>
    /// <param name="id">The ID of the operation to remove.</param>
    void Remove(Guid id);

    /// <summary>
    /// Tries to get an operation by ID.
    /// </summary>
    /// <param name="id">The ID of the operation to get.</param>
    /// <param name="operation">The operation, if found.</param>
    /// <returns>True if the operation was found; otherwise, false.</returns>
    bool TryGetOperation(Guid id, out OperationContext? operation);
}
