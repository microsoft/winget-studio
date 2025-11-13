// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using WinGetStudio.Services.Operations.Contracts;

namespace WinGetStudio.Services.Operations.Models.States;

public sealed record class OperationExecutionOptions(IReadOnlyList<IOperationPolicy> Policies);
