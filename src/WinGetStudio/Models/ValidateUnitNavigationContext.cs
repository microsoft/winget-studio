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
    /// Gets the preview set that contains the original unit.
    /// </summary>
    public PreviewSetViewModel? SourcePreviewSet { get; }

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
        PreviewSetViewModel? sourcePreviewSet = null)
    {
        UnitToValidate = unitToValidate;
        SourceUnit = sourceUnit;
        SourcePreviewSet = sourcePreviewSet;
    }
}
