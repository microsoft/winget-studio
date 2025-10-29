// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Models;

namespace WinGetStudio.Services;

internal sealed partial class ConfigurationManager : IConfigurationManager
{
    private readonly ILogger<ConfigurationManager> _logger;

    /// <inheritdoc/>
    public SetPreviewState ActiveSetPreviewState { get; set; }

    /// <inheritdoc/>
    public SetApplyState ActiveSetApplyState { get; set; }

    public ConfigurationManager(ILogger<ConfigurationManager> logger)
    {
        _logger = logger;
        ActiveSetPreviewState = new(_logger);
        ActiveSetApplyState = new(_logger);
    }
}
