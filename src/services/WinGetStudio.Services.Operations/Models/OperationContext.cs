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

    public OperationContext(ILogger<OperationContext> logger, IOperationPublisher publisher)
    {
        _logger = logger;
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

            _logger.LogDebug($"Operation {Id} updated: {_currentSnapshot.Properties}. Publishing snapshots.");
            _publisher.PublishSnapshots();
        }
    }

    public void Notify(Func<OperationProperties, OperationProperties>? mutate = null)
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
            _publisher.PublishNotification(notificationProps);
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
