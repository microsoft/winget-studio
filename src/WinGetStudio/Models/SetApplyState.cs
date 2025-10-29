// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using WinGetStudio.Contracts.Models;
using WinGetStudio.ViewModels;
using WinGetStudio.ViewModels.ConfigurationFlow;

namespace WinGetStudio.Models;

/// <summary>
/// Represents the state of the currently applied set.
/// </summary>
public sealed partial class SetApplyState : ISessionStateAware<ApplyFileViewModel>
{
    private readonly ILogger _logger;

    /// <summary>
    /// Gets or sets the active apply set.
    /// </summary>
    public ApplySetViewModel? ActiveApplySet { get; set; }

    public SetApplyState(ILogger logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public bool CanRestoreState()
    {
        return ActiveApplySet != null;
    }

    /// <inheritdoc/>
    public void CaptureState(ApplyFileViewModel source)
    {
        _logger.LogInformation("Capturing apply set state");
        ActiveApplySet = source.ApplySet;
    }

    /// <inheritdoc/>
    public void RestoreState(ApplyFileViewModel source)
    {
        _logger.LogInformation("Restoring apply set state");
        source.ApplySet = ActiveApplySet;
    }

    /// <inheritdoc/>
    public void ClearState()
    {
        _logger.LogInformation("Clearing apply set state");
        ActiveApplySet = null;
    }
}
