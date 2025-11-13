// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.Services.Operations.Models.States;

/// <summary>
/// Represents global activity state.
/// </summary>
/// <param name="Percent">The percent complete or null if indeterminate.</param>
/// <param name="InProgressCount">The number of operations in progress.</param>
public sealed record class GlobalActivity(int? Percent, int InProgressCount);
