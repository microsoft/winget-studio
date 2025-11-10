// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace WinGetStudio.Services.Operations.Contracts;

public interface IOperation
{
    Task ExecuteAsync(IOperationContext ctx);
}
