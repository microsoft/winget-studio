// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using WinGetStudio.CLI.Contracts;

namespace WinGetStudio.CLI.Services;

internal sealed class OptionFactory : IOptionFactory
{
    private readonly IServiceProvider _sp;

    public OptionFactory(IServiceProvider sp)
    {
        _sp = sp;
    }

    /// <inheritdoc/>
    public T Create<T>()
        where T : Option => _sp.GetRequiredService<T>();
}
