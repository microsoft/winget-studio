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
    /// Gets the set that contains the source unit.
    /// </summary>
    public SetViewModel? SourceSet { get; }

    /// <summary>
    /// Gets the source unit.
    /// </summary>
    public UnitViewModel? SourceUnit { get; }

    /// <summary>
    /// Gets the unit to validate.
    /// </summary>
    public UnitViewModel UnitToValidate { get; }

    public ValidateUnitNavigationContext(
        UnitViewModel unitToValidate,
        UnitViewModel? sourceUnit = null,
        SetViewModel? sourceSet = null)
    {
        UnitToValidate = unitToValidate;
        SourceUnit = sourceUnit;
        SourceSet = sourceSet;
    }
}
