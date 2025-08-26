// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Contracts.Services;
using WinGetStudio.ViewModels;
using WinGetStudio.Views;

namespace WinGetStudio.Services;

internal sealed class ValidationPageService : PageService, IValidationPageService
{
    protected override void ConfigurePages()
    {
        Configure<ValidationViewModel, ValidationPage>();
        Configure<ValidationFrameViewModel, ValidationFramePage>();
    }
}
