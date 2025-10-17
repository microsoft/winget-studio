// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.ViewModels;

namespace WinGetStudio.Models;

public sealed partial class ValidateUnitNavigationContext
{
    public UnitViewModel UnitToValidate { get; }

    public ValidateUnitNavigationContext(UnitViewModel unitToValidate)
    {
        UnitToValidate = unitToValidate;
    }
}
