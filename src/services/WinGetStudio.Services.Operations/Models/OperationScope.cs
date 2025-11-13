// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.Services.Operations.Models;

internal delegate OperationScope OperationScopeFactory(OperationExecutionOptions options, CancellationToken cancellationToken);

internal sealed partial class OperationScope : IOperationScope
{
    private readonly ILogger<OperationScope> _logger;
    private readonly IOperationManager _manager;
    private readonly OperationContext _context;
    private readonly OperationExecutionOptions _options;
    private bool _disposedValue;

    /// <inheritdoc/>
    public IOperationContext Context => _context;

    public OperationScope(
        ILogger<OperationScope> logger,
        OperationContextFactory contextFactory,
        IOperationManager manager,
        OperationExecutionOptions options,
        CancellationToken cancellationToken)
    {
        _logger = logger;
        _context = contextFactory(cancellationToken);
        _options = options;
        _manager = manager;
    }

    /// <summary>
    /// Begins the operation scope.
    /// </summary>
    public async Task BeginAsync()
    {
        ObjectDisposedException.ThrowIf(_disposedValue, nameof(OperationScope));
        _logger.LogInformation($"Beginning operation scope for operation id: {_context.Id}");
        _manager.AddActiveOperationContext(_context);
        await _manager.ApplyStartPoliciesAsync(_options.Policies, _context);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (!_disposedValue)
        {
            _disposedValue = true;
            await _manager.ApplyCompletionPoliciesAsync(_options.Policies, _context);
            _manager.RemoveActiveOperationContext(_context.Id);
            _context.Dispose();
            _logger.LogInformation($"Disposed operation scope for operation id: {_context.Id}");
        }
    }
}
