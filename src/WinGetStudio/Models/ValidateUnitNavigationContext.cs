// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.ViewModels;

namespace WinGetStudio.Models;

public sealed partial class ValidateUnitNavigationContext
{
    public DSCUnitViewModel UnitToValidate { get; }

    public ValidateUnitNavigationContext(DSCUnitViewModel unitToValidate)
    {
        UnitToValidate = unitToValidate;
    }
}
