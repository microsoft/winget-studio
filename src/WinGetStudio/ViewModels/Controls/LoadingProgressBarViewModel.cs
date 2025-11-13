// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.ViewModels.Controls;

public partial class LoadingProgressBarViewModel : ObservableRecipient
{
    [ObservableProperty]
    public partial int ProgressValue { get; set; }

    [ObservableProperty]
    public partial bool IsProgressIndeterminate { get; set; }

    [ObservableProperty]
    public partial bool IsProgressVisible { get; set; }

    public LoadingProgressBarViewModel(IOperationHub ops)
    {
        ops.GlobalActivity.Subscribe(OnGlobalActivity);
    }

    /// <summary>
    /// Handles global activity updates.
    /// </summary>
    /// <param name="activity">The global activity.</param>
    private void OnGlobalActivity(GlobalActivity activity)
    {
        IsProgressVisible = activity.InProgressCount > 0;
        IsProgressIndeterminate = activity.Percent == null;
        ProgressValue = activity.Percent ?? 0;
    }
}
