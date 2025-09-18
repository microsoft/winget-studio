// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace WinGetStudio.CLI.Contracts;

internal interface IOptionFactory
{
    /// <summary>
    /// Creates an option of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of option to create.</typeparam>
    /// <returns>The option instance.</returns>
    T Create<T>()
        where T : Option;
}
