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

    /// <summary>
    /// Gets or sets the active set.
    /// </summary>
    public SetViewModel? ActiveSet { get; set; }

    public SetPreviewState(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets or sets the selected unit tuple.
    /// </summary>
    public Tuple<UnitViewModel, UnitViewModel>? SelectedUnit { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the preview is in edit mode.
    /// </summary>
    public bool IsEditMode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the preview is in code view mode.
    /// </summary>
    public bool IsCodeView { get; set; }

    /// <inheritdoc/>
    public bool CanRestoreState()
    {
        return ActiveSet != null;
    }

    /// <inheritdoc/>
    public void CaptureState(PreviewFileViewModel source)
    {
        _logger.LogInformation("Capturing preview set state");
        ActiveSet = source.ConfigurationSet;
        IsCodeView = source.IsCodeView;
        IsEditMode = source.IsEditMode;
        SelectedUnit = source.SelectedUnit;
    }

    /// <inheritdoc/>
    public void RestoreState(PreviewFileViewModel source)
    {
        _logger.LogInformation("Restoring preview set state");
        source.ConfigurationSet = ActiveSet;
        source.IsCodeView = IsCodeView;
        source.IsEditMode = IsEditMode;
        source.SelectedUnit = SelectedUnit;
    }

    /// <inheritdoc/>
    public void ClearState()
    {
        _logger.LogInformation("Clearing preview set state");
        ActiveSet = null;
        SelectedUnit = null;
        IsEditMode = false;
        IsCodeView = false;
    }
}
