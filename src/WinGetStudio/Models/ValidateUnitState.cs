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
    /// Gets or sets the active validate unit.
    /// </summary>
    public ValidateUnitViewModel? ActiveValidateUnit { get; set; }

    public ValidateUnitState(ILogger logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public bool CanRestoreState()
    {
        return ActiveValidateUnit != null;
    }

    /// <inheritdoc/>
    public void CaptureState(ValidationViewModel source)
    {
        _logger.LogInformation("Capturing validation state");
        ActiveValidateUnit = source.SelectedUnit;
    }

    /// <inheritdoc/>
    public void RestoreState(ValidationViewModel source)
    {
        _logger.LogInformation("Restoring validation state");
        source.SelectedUnit = ActiveValidateUnit;
    }

    /// <inheritdoc/>
    public void ClearState()
    {
        _logger.LogInformation("Clearing validation state");
        ActiveValidateUnit = null;
    }
}
