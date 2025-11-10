// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using WinGetStudio.Services.Operations.Models.State;

namespace WinGetStudio.Services.Operations.Contracts;

public interface IOperationPublisher
{
    void PublishSnapshots(IReadOnlyList<OperationSnapshot> snapshots);

    void PublishEvent(OperationProperties properties);
}
