// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Services.Settings.Models;

namespace WinGetStudio.Services.Settings.Contracts;

public interface IGeneralSettings
{
    string Theme { get; }

    public GeneralSettings Clone();
}
