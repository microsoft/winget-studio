// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Contracts.Services;

namespace WinGetStudio.Extensions;

public static class ResourceExtensions
{
    private static readonly IStringResource _stringResource = App.GetService<IStringResource>();

    public static string GetLocalized(this string resourceKey, params object[] args) => _stringResource.GetLocalized(resourceKey, args);
}
