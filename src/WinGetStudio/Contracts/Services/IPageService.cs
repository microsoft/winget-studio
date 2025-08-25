// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;

namespace WinGetStudio.Contracts.Services;

public interface IPageService
{
    Type GetPageType(Type viewModelType);

    Type GetPageType<TVM>()
        where TVM : ObservableObject;
}
