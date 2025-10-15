// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Models;

namespace WinGetStudio.Contracts.Services;

public interface IConfigurationSetManager
{
    ConfigurationSetPreviewState ActivePreviewState { get; set; }
}
