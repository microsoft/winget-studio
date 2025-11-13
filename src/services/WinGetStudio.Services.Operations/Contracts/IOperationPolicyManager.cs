// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace WinGetStudio.Services.Operations.Contracts;

internal interface IOperationPolicyManager
{
    /// <summary>
    /// Applies the given policies with the specified type to the operation context.
    /// </summary>
    /// <typeparam name="T">The type of policy to apply.</typeparam>
    /// <param name="policies">The list of policies to filter and apply.</param>
    /// <param name="context">The operation context.</param>
    Task ApplyPoliciesAsync<T>(IReadOnlyList<IOperationPolicy>? policies, IOperationContext context)
        where T : IOperationPolicy;
}
