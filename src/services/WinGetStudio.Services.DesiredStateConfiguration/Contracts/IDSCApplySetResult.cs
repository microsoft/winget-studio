// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace WinGetStudio.Services.DesiredStateConfiguration.Contracts;

public interface IDSCApplySetResult
{
    /// <summary>
    /// Gets the configuration set that was applied.
    /// </summary>
     IDSCSet AppliedSet { get; }

    /// <summary>
    /// Gets the overall result from applying the configuration set.
    /// </summary>
    Exception ResultCode { get; }

    /// <summary>
    /// Gets the results of the individual units in the configuration file.
    /// </summary>
    public IReadOnlyList<IDSCApplyUnitResult> UnitResults { get; }
}
