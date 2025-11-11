// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models.State;

namespace WinGetStudio.Services.Operations.Models;

internal delegate OperationContext OperationContextFactory();

internal sealed partial class OperationContext : IOperationContext, IDisposable
{
    private readonly object _lock = new();
    private readonly ILogger<OperationContext> _logger;
    private readonly IOperationManager _manager;
    private readonly CancellationTokenSource _cts;
    private OperationSnapshot _currentSnapshot;
    private bool _disposedValue;

    /// <inheritdoc/>
    public Guid Id => _currentSnapshot.Id;

    /// <inheritdoc/>
    public CancellationToken CancellationToken => _cts.Token;

    /// <inheritdoc/>
    public OperationSnapshot CurrentSnapshot => _currentSnapshot;

    public OperationContext(
        ILogger<OperationContext> logger,
        IOperationManager manager)
    {
        _logger = logger;
        _manager = manager;
        _cts = new();
        _currentSnapshot = OperationSnapshot.Empty with
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <inheritdoc/>
    public void CommitSnapshot(Func<OperationProperties, OperationProperties> mutate)
    {
        if (!_disposedValue)
        {
            OperationProperties props;
            lock (_lock)
            {
                _currentSnapshot = _currentSnapshot with
                {
                    Properties = mutate(_currentSnapshot.Properties),
                    UpdatedAt = DateTimeOffset.UtcNow,
                };
                props = _currentSnapshot.Properties;
            }

            _logger.LogDebug($"Operation {Id} snapshot committed: {props}. Publishing snapshots.");
            _manager.PublishSnapshots();
        }
    }

    public void PublishNotification(Func<OperationProperties, OperationProperties>? mutate = null)
    {
        if (!_disposedValue)
        {
            OperationNotification notificationProps;
            lock (_lock)
            {
                var currentProps = _currentSnapshot.Properties;
                var newProps = mutate != null ? mutate(currentProps) : currentProps;
                notificationProps = new OperationNotification(Id, newProps);
            }

            _logger.LogDebug($"Operation {Id} notification: {notificationProps.Properties}. Publishing notification.");
            _manager.PublishNotification(notificationProps);
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

    /// <inheritdoc/>
    public void Publish() => _manager.Publish(this);

    /// <inheritdoc/>
    public void Unpublish() => _manager.Unpublish(this);

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
