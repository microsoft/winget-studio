// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using WinGetStudio.Contracts.Models;
using WinGetStudio.ViewModels;

namespace WinGetStudio.Models;

/// <summary>
/// Represents the state of the current validation.
/// </summary>
public sealed partial class ValidateUnitState : ISessionStateAware<ValidationViewModel>
{
    private readonly ILogger _logger;

    /// <summary>
    /// Gets or sets the validate unit list.
    /// </summary>
    private List<ValidateUnitViewModel> ValidateUnitList { get; set; } = [];

    /// <summary>
    /// Gets or sets the selected unit.
    /// </summary>
    private ValidateUnitViewModel? SelectedUnit { get; set; }

    public ValidateUnitState(ILogger logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public bool CanRestoreState()
    {
        return ValidateUnitList != null && SelectedUnit != null;
    }

    /// <inheritdoc/>
    public void CaptureState(ValidationViewModel source)
    {
        _logger.LogInformation("Capturing validation state");
        ValidateUnitList = [..source.ValidateUnitList];
        SelectedUnit = source.SelectedUnit;
    }

    /// <inheritdoc/>
    public void RestoreState(ValidationViewModel source)
    {
        _logger.LogInformation("Restoring validation state");
        foreach (var item in ValidateUnitList)
        {
            source.ValidateUnitList.Add(item);
        }

        source.SelectedUnit = SelectedUnit;
    }

    /// <inheritdoc/>
    public void ClearState()
    {
        _logger.LogInformation("Clearing validation state");
        SelectedUnit = null;
        ValidateUnitList.Clear();
    }
}
