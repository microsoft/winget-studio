// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace WinGetStudio.Services.Operations.Models;

public sealed record class OperationSnapshot(
    Guid Id,
    string Title,
    string Message,
    int? Percent,
    );
