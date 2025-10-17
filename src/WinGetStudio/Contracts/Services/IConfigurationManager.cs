// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Models;

namespace WinGetStudio.Contracts.Services;

public interface IConfigurationManager
{
    SetPreviewState ActiveSetPreviewState { get; set; }

    SetApplyState ActiveSetApplyState { get; set; }
}
