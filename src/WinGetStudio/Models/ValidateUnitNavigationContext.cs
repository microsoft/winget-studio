// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.ViewModels;

namespace WinGetStudio.Models;

/// <summary>
/// Represents the navigation context for validating a unit.
/// </summary>
public sealed partial class ValidateUnitNavigationContext
{
    /// <summary>
    /// Gets the unit to validate.
    /// </summary>
    public UnitViewModel UnitToValidate { get; }

    public ValidateUnitNavigationContext(UnitViewModel unitToValidate)
    {
        UnitToValidate = unitToValidate;
    }
}
