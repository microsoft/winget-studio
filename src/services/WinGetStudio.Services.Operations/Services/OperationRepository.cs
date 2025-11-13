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
    private readonly ConcurrentDictionary<Guid, OperationContext> _activeOperationContexts;
    private readonly ConcurrentDictionary<Guid, OperationSnapshot> _operationSnapshots;

    /// <inheritdoc/>
    public IReadOnlyList<OperationContext> ActiveOperationContexts => [.._activeOperationContexts.Values];

    /// <inheritdoc/>
    public IReadOnlyList<OperationSnapshot> OperationSnapshots => [.._operationSnapshots.Values.OrderBy(snapshot => snapshot.CreatedAt)];

    public OperationRepository()
    {
        _activeOperationContexts = [];
        _operationSnapshots = [];
    }

    /// <inheritdoc/>
    public void AddActiveOperationContext(OperationContext operation) => _activeOperationContexts.TryAdd(operation.Id, operation);

    /// <inheritdoc/>
    public void RemoveActiveOperationContext(Guid id) => _activeOperationContexts.TryRemove(id, out _);

    /// <inheritdoc/>
    public void AddOperationSnapshot(OperationSnapshot snapshot) => _operationSnapshots.TryAdd(snapshot.Id, snapshot);

    /// <inheritdoc/>
    public void RemoveOperationSnapshot(Guid id) => _operationSnapshots.TryRemove(id, out _);

    /// <inheritdoc/>
    public void UpdateOperationSnapshot(OperationSnapshot snapshot) => _operationSnapshots.AddOrUpdate(snapshot.Id, snapshot, (_, _) => snapshot);

    /// <inheritdoc/>
    public bool ContainsOperationSnapshot(Guid id) => _operationSnapshots.ContainsKey(id);
}
