// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

namespace WinGetStudio.Services.Operations.Models.State;

public sealed record class OperationAction(string Text, bool IsPrimary, Func<Task> Action);
