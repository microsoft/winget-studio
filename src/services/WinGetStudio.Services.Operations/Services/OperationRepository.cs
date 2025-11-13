// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.Services.Operations.Services;

internal sealed class OperationRepository : IOperationRepository
{
    private readonly ConcurrentDictionary<Guid, OperationContext> _operations;

    /// <inheritdoc/>
    public IReadOnlyList<OperationContext> Operations => [.._operations.Values.OrderBy(op => op.CurrentSnapshot.CreatedAt)];

    /// <inheritdoc/>
    public IReadOnlyList<OperationSnapshot> Snapshots => [.. Operations.Select(op => op.CurrentSnapshot)];

    public OperationRepository()
    {
        _operations = [];
    }

    /// <inheritdoc/>
    public void Add(OperationContext operation) => _operations.TryAdd(operation.Id, operation);

    /// <inheritdoc/>
    public void Remove(OperationContext operation) => _operations.TryRemove(operation.Id, out _);

    /// <inheritdoc/>
    public bool TryGetOperation(Guid id, out OperationContext? operation) => _operations.TryGetValue(id, out operation);
}
