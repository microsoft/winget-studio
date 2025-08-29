// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using WinGetStudio.Services.Core.Extensions;
using WinGetStudio.Services.Telemetry.Contracts;

namespace WinGetStudio.Services.Telemetry.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddTelemetry(this IServiceCollection services)
    {
        services.AddCore();
        services.AddSingleton<ITelemetry, Services.Telemetry>();
        return services;
    }
}
