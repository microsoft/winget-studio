// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using WinGetStudio.Services.Operations.Models.State;

namespace WinGetStudio.Services.Operations.Contracts;

public interface IOperationContext
{
    Guid Id { get; }

    CancellationToken CancellationToken { get; }

    OperationSnapshot CurrentSnapshot { get; }

    void Update(Func<OperationProperties, OperationProperties> mutate);

    void Cancel();
}
