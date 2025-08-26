// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
