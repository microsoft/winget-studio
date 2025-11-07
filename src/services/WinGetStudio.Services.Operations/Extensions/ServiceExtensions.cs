// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using WinGetStudio.Services.Core.Extensions;

namespace WinGetStudio.Services.Operations.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddLogging(this IServiceCollection services, string appSettingsFileName)
    {
        services.AddCore();
        return services;
    }
}
