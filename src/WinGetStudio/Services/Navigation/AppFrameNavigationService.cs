// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Events;
using WinGetStudio.Services.Telemetry.Contracts;
using WinGetStudio.ViewModels;

namespace WinGetStudio.Services.Navigation;

internal sealed class AppFrameNavigationService : FrameNavigationService, IAppFrameNavigationService
{
    private readonly ITelemetryService _telemetryService;

    public AppFrameNavigationService(IAppPageService pageService, ITelemetryService telemetryService)
        : base(pageService)
    {
        _telemetryService = telemetryService;
    }

    public bool NavigateToDefaultPage(object? parameter = null, bool clearNavigation = false)
    {
        return NavigateTo<MainViewModel>(parameter, clearNavigation);
    }

    public override bool NavigateTo(Type pageKey, object? parameter = null, bool clearNavigation = false)
    {
        var result = base.NavigateTo(pageKey, parameter, clearNavigation);
        _telemetryService.WriteEvent(new NavigatedToPageEvent(pageKey.Name)
        {
            IsSuccessful = result,
        });
        return result;
    }

    protected override Frame? GetDefaultFrame()
    {
        return App.MainWindow.Content as Frame;
    }
}
