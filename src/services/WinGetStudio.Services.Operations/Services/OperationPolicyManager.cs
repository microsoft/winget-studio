// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WinGetStudio.Services.Operations.Contracts;

namespace WinGetStudio.Services.Operations.Services;

internal sealed partial class OperationPolicyManager : IOperationPolicyManager
{
    private readonly ILogger<OperationPolicyManager> _logger;

    public OperationPolicyManager(ILogger<OperationPolicyManager> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task ApplyPoliciesAsync<T>(IReadOnlyList<IOperationPolicy>? policies, IOperationContext context)
        where T : IOperationPolicy
    {
        var policyCandidates = policies?.OfType<T>().ToList();
        if (policyCandidates == null || policyCandidates.Count == 0)
        {
            return;
        }

        foreach (var policy in policyCandidates)
        {
            if (policy.CanApply(context))
            {
                _logger.LogInformation($"Applying policy {policy.GetType().Name} to operation {context.Id}.");
                await policy.ApplyAsync(context);
            }
        }
    }
}
