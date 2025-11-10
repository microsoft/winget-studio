// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.Services.Operations.Models.State;

public sealed record class GlobalActivity(int? Percent, int InProgressCount);
