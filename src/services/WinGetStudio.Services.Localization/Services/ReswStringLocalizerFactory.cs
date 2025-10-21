// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.Localization;

namespace WingetStudio.Services.Localization.Services;

internal sealed class ReswStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly IStringLocalizer _localizer;

    public ReswStringLocalizerFactory(IStringLocalizer localizer)
    {
        _localizer = localizer;
    }

    /// <inheritdoc/>
    public IStringLocalizer Create(Type resourceSource) => _localizer;

    /// <inheritdoc/>
    public IStringLocalizer Create(string baseName, string location) => _localizer;
}
