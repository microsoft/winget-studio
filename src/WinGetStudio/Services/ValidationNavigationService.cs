// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Contracts.Services;
using WinGetStudio.ViewModels;

namespace WinGetStudio.Services;

internal sealed class ValidationNavigationService : NavigationService, IValidationNavigationService
{
    public ValidationNavigationService(IValidationPageService pageService)
        : base(pageService)
    {
    }

    public bool NavigateToDefaultPage(object? parameter = null, bool clearNavigation = false)
    {
        return NavigateTo<ValidationViewModel>(parameter, clearNavigation);
    }
}
