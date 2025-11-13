// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace WinGetStudio.Services.Operations.Contracts;

public interface IOperationPolicy
{
    /// <summary>
    /// Checks whether the policy can be applied to the given operation context.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <returns>>True if the policy can be applied; otherwise, false.</returns>
    bool CanApply(IOperationContext context);

    /// <summary>
    /// Applies the policy to the given operation context.
    /// </summary>
    /// <param name="context">The operation context.</param>
    Task ApplyAsync(IOperationContext context);
}
