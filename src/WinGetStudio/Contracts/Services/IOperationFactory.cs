// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Models.Operations;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

namespace WinGetStudio.Contracts.Services;

public interface IOperationFactory
{
    GetUnitOperation CreateGetUnitOperation(IDSCFile dscFile);

    SetUnitOperation CreateSetUnitOperation(IDSCFile dscFile);

    TestUnitOperation CreateTestUnitOperation(IDSCFile dscFile);

    ValidateSetOperation CreateValidateSetOperation(IDSCFile dscFile);

    ApplySetOperation CreateApplySetOperation(IDSCSet dscSet, IProgress<IDSCSetChangeData> progress);

    TestSetOperation CreateTestSetOperation(IDSCFile dscFile);

    OpenSetOperation CreateOpenSetOperation(IDSCFile dscFile);
}
