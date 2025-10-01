// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using WinGetStudio.CLI.Contracts;

namespace WinGetStudio.CLI.Services;

internal sealed class CommandFactory : ICommandFactory
{
    private readonly IServiceProvider _sp;

    public CommandFactory(IServiceProvider sp)
    {
        _sp = sp;
    }

    /// <inheritdoc/>
    public T Create<T>()
        where T : Command => _sp.GetRequiredService<T>();
}
