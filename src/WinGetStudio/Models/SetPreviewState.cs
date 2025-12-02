// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using WinGetStudio.Contracts.Models;
using WinGetStudio.ViewModels;
using WinGetStudio.ViewModels.ConfigurationFlow;

namespace WinGetStudio.Models;

/// <summary>
/// Represents the state of the current set preview.
/// </summary>
public sealed partial class SetPreviewState : ISessionStateAware<PreviewFileViewModel>
{
    private readonly ILogger _logger;

    public PreviewSetViewModel? ActiveSet { get; set; }

    public SetPreviewState(ILogger logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public bool CanRestoreState()
    {
        return ActiveSet != null;
    }

    /// <inheritdoc/>
    public void CaptureState(PreviewFileViewModel source)
    {
        _logger.LogInformation("Capturing preview set state");
        ActiveSet = source.Set;
    }

    /// <inheritdoc/>
    public void RestoreState(PreviewFileViewModel source)
    {
        _logger.LogInformation("Restoring preview set state");
        if (ActiveSet != null)
        {
            source.Set = ActiveSet;
        }
    }

    /// <inheritdoc/>
    public void ClearState()
    {
        _logger.LogInformation("Clearing preview set state");
        ActiveSet = null;
    }
}
