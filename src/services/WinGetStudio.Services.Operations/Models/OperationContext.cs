// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models.State;

namespace WinGetStudio.Services.Operations.Models;

internal delegate OperationContext OperationContextFactory();

internal sealed partial class OperationContext : IOperationContext, IDisposable
{
    private readonly object _lock = new();
    private readonly IOperationPublisher _publisher;
    private readonly CancellationTokenSource _cts;
    private OperationSnapshot _currentSnapshot;
    private bool _disposedValue;

    /// <inheritdoc/>
    public Guid Id => _currentSnapshot.Id;

    /// <inheritdoc/>
    public CancellationToken CancellationToken => _cts.Token;

    /// <inheritdoc/>
    public OperationSnapshot CurrentSnapshot => _currentSnapshot;

    public OperationContext(IOperationPublisher publisher)
    {
        _publisher = publisher;
        _cts = new();
        _currentSnapshot = OperationSnapshot.Empty with
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <inheritdoc/>
    public void Update(Func<OperationProperties, OperationProperties> mutate)
    {
        if (!_disposedValue)
        {
            lock (_lock)
            {
                _currentSnapshot = _currentSnapshot with
                {
                    Properties = mutate(_currentSnapshot.Properties),
                    UpdatedAt = DateTimeOffset.UtcNow,
                };
            }

            _publisher.PublishSnapshots();
        }
    }

    /// <inheritdoc/>
    public void Cancel()
    {
        if (!_disposedValue)
        {
            _cts.Cancel();
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
