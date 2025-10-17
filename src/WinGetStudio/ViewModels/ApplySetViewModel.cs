// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Localization;
using WinGetStudio.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

namespace WinGetStudio.ViewModels;

public sealed partial class ApplySetViewModel : ObservableObject
{
    public ObservableCollection<ApplyUnitViewModel> Units { get; }

    public ApplySetViewModel(IStringLocalizer localizer, IDSCSet applySet)
    {
        Units = [..applySet.Units.Select(unit => new ApplyUnitViewModel(localizer, unit))];
    }
}
