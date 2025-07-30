// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.Contracts.Views;

public interface IView<T>
{
    public T ViewModel { get; }
}
