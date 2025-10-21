// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using WinGetStudio.Services.Core.Extensions;
using WinGetStudio.Services.Telemetry.Contracts;
using WinGetStudio.Services.Telemetry.Services;

namespace WinGetStudio.Services.Telemetry.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddTelemetry(this IServiceCollection services)
    {
        services.AddCore();
        services.AddSingleton<ITelemetryService, TelemetryService>();
        return services;
    }
}
