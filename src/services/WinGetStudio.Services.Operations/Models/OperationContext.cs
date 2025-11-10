// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models.State;
using WinGetStudio.Services.Operations.Services;

namespace WinGetStudio.Services.Operations.Models;

internal sealed partial class OperationContext : IOperationContext, IDisposable
{
    private readonly OperationManager _manager;
    private readonly CancellationTokenSource _cts;
    private OperationSnapshot _currentSnapshot;
    private bool _disposedValue;

    public Guid Id => _currentSnapshot.Id;

    public CancellationToken CancellationToken => _cts.Token;

    public OperationSnapshot CurrentSnapshot => _currentSnapshot;

    public OperationContext(OperationManager manager)
    {
        _manager = manager;
        _cts = new();
        _currentSnapshot = OperationSnapshot.Empty with
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    public void Update(Func<OperationProperties, OperationProperties> mutate)
    {
        if (_disposedValue)
        {
            _currentSnapshot = _currentSnapshot with
            {
                Properties = mutate(_currentSnapshot.Properties),
                UpdatedAt = DateTimeOffset.UtcNow,
            };
            _manager.PublishSnapshot(Id);
        }
    }

    public void Cancel()
    {
        if (!_disposedValue)
        {
            _cts.Cancel();
            Update(props => props with { Status = OperationStatus.Canceled });
        }
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _cts.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
