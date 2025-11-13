// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using WinGetStudio.Services.Operations.Models;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.Services.Operations.Contracts;

internal interface IOperationRepository
{
    /// <summary>
    /// Gets the list of operation snapshots.
    /// </summary>
    IReadOnlyList<OperationSnapshot> OperationSnapshots { get; }

    /// <summary>
    /// Gets the list of all active operation contexts.
    /// </summary>
    IReadOnlyList<OperationContext> ActiveOperationContexts { get; }

    /// <summary>
    /// Adds an active operation context to the collection.
    /// </summary>
    /// <param name="operation">The operation to add.</param>
    void AddActiveOperationContext(OperationContext operation);

    /// <summary>
    /// Removes the specified active operation context from the collection.
    /// </summary>
    /// <param name="id">The identifier of the operation context to remove.</param>
    void RemoveActiveOperationContext(Guid id);

    /// <summary>
    /// Adds an operation snapshot to the repository.
    /// </summary>
    /// <param name="snapshot">The operation snapshot to add.</param>
    void AddOperationSnapshot(OperationSnapshot snapshot);

    /// <summary>
    /// Removes an operation snapshot from the repository.
    /// </summary>
    /// <param name="id">The identifier of the operation snapshot to remove.</param>
    void RemoveOperationSnapshot(Guid id);

    /// <summary>
    /// Updates an operation snapshot in the repository.
    /// </summary>
    /// <param name="snapshot">The operation snapshot to update.</param>
    void UpdateOperationSnapshot(OperationSnapshot snapshot);

    /// <summary>
    /// Checks if the repository contains an operation snapshot with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the operation snapshot.</param>
    /// <returns>True if the operation snapshot exists; otherwise, false.</returns>
    bool ContainsOperationSnapshot(Guid id);
}
