// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using WinGetStudio.Services.Core.Extensions;
using WingetStudio.Services.Localization.Services;

namespace WingetStudio.Services.Localization.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddReswLocalization(this IServiceCollection services)
    {
        services.AddCore();

        // Register built-in localizer services
        // Reference 1: https://github.com/dotnet/aspnetcore/blob/main/src/Localization/Localization/src/LocalizationServiceCollectionExtensions.cs
        // Reference 2: https://github.com/dotnet/aspnetcore/blob/main/src/Localization/Abstractions/src/StringLocalizerOfT.cs
        services.AddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));

        // Register adapter localizer services for resw files
        services.AddSingleton<IStringLocalizer, ReswStringLocalizer>();
        services.AddSingleton<IStringLocalizerFactory, ReswStringLocalizerFactory>();
        return services;
    }
}
