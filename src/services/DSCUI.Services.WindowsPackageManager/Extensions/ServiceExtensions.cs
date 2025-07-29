// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using DSCUI.Services.Core.Extensions;
using DSCUI.Services.WindowsPackageManager.COM;
using DSCUI.Services.WindowsPackageManager.Contracts;
using DSCUI.Services.WindowsPackageManager.Contracts.Operations;
using DSCUI.Services.WindowsPackageManager.Services;
using DSCUI.Services.WindowsPackageManager.Services.Operations;
using Microsoft.Extensions.DependencyInjection;

namespace DSCUI.Services.WindowsPackageManager.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddWinGet(this IServiceCollection services, bool useDev = false)
    {
        var context = useDev ? ClsidContext.Dev : ClsidContext.Prod;
        services.AddSingleton<WindowsPackageManagerFactory>(new WindowsPackageManagerDefaultFactory(context));
        services.AddWinGetCommon();
        return services;
    }

    public static IServiceCollection AddWinGetElevated(this IServiceCollection services)
    {
        services.AddSingleton<WindowsPackageManagerFactory>(new WindowsPackageManagerManualActivationFactory());
        services.AddWinGetCommon();
        return services;
    }

    private static void AddWinGetCommon(this IServiceCollection services)
    {
        services.AddCore();
        services.AddSingleton<IWinGet, WinGet>();
        services.AddSingleton<IWinGetCatalogConnector, WinGetCatalogConnector>();
        services.AddSingleton<IWinGetPackageFinder, WinGetPackageFinder>();
        services.AddSingleton<IWinGetPackageInstaller, WinGetPackageInstaller>();
        services.AddSingleton<IWinGetProtocolParser, WinGetProtocolParser>();
        services.AddSingleton<IWinGetDeployment, WinGetDeployment>();
        services.AddSingleton<IWinGetRecovery, WinGetRecovery>();
        services.AddSingleton<IWinGetPackageCache, WinGetPackageCache>();
        services.AddSingleton<IWinGetOperations, WinGetOperations>();
        services.AddSingleton<IWinGetGetPackageOperation, WinGetGetPackageOperation>();
        services.AddSingleton<IWinGetSearchOperation, WinGetSearchOperation>();
        services.AddSingleton<IWinGetInstallOperation, WinGetInstallOperation>();
    }
}
