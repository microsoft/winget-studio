// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace WinGetStudio.CLI.Contracts;

internal interface ICommandFactory
{
    /// <summary>
    /// Creates a command of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of command to create.</typeparam>
    /// <returns>The command instance.</returns>
    T Create<T>()
        where T : Command;
}
