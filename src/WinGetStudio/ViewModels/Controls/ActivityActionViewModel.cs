// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetStudio.Services.Operations.Models.States;

namespace WinGetStudio.ViewModels.Controls;

public sealed partial class ActivityActionViewModel : ObservableObject
{
    public Guid Id { get; }

    [ObservableProperty]
    public partial string? Text { get; set; }

    [ObservableProperty]
    private partial Func<Task>? Action { get; set; }

    [ObservableProperty]
    public partial bool IsPrimary { get; set; } = false;

    public ActivityActionViewModel(OperationAction action)
    {
        Id = action.Id;
        Update(action);
    }

    [RelayCommand]
    private async Task OnExecuteAsync()
    {
        if (Action != null)
        {
            await Action();
        }
    }

    /// <summary>
    /// Update the action.
    /// </summary>
    /// <param name="action">The action.</param>
    public void Update(OperationAction action)
    {
        Text = action.Text;
        Action = action.Action;
        IsPrimary = action.IsPrimary;
    }
}
