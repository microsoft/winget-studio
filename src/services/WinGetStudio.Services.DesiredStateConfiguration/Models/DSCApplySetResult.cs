// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using Microsoft.Management.Configuration;

namespace WinGetStudio.Services.DesiredStateConfiguration.Models;

internal sealed class DSCApplySetResult : IDSCApplySetResult
{
    public DSCApplySetResult(IDSCSet appliedSet, ApplyConfigurationSetResult result)
    {
        // Constructor copies all the required data from the out-of-proc COM
        // objects over to the current process. This ensures that we have this
        // information available even if the out-of-proc COM objects are no
        // longer available (e.g. AppInstaller service is no longer running).
        AppliedSet = appliedSet;
        ResultCode = result.ResultCode;
        UnitResults = result.UnitResults.Select(unitResult => new DSCApplyUnitResult(unitResult)).ToList();
    }

    public IDSCSet AppliedSet { get; }

    public Exception ResultCode { get; }

    public IReadOnlyList<IDSCApplyUnitResult> UnitResults { get; }
}
