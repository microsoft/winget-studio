// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.Contracts.Services;

public interface IStringResource
{
    /// <summary>
    /// Gets a localized string based on the provided key.
    /// </summary>
    /// <param name="key">Key for the localized string.</param>
    /// <param name="args">Optional arguments to format the localized string.</param>
    /// <returns>A localized string with formatted arguments.</returns>
    string GetLocalized(string key, params object[] args);
}
