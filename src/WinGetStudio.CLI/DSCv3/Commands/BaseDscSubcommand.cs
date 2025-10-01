// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Threading.Tasks;
using Windows.Win32.Foundation;
using WinGetStudio.CLI.Contracts;
using WinGetStudio.CLI.DSCv3.Contracts;
using WinGetStudio.CLI.DSCv3.DscResources;
using WinGetStudio.CLI.DSCv3.Options;

namespace WinGetStudio.CLI.DSCv3.Commands;

internal abstract class BaseDscSubcommand : Command
{
    private readonly InputOption _inputOption;
    private readonly ResourceOption _resourceOption;

    protected IOptionFactory OptionFactory { get; }

    protected IResourceProvider ResourceProvider { get; }

    /// <summary>
    /// Gets the DSC resource to be used by the command.
    /// </summary>
    protected BaseResource Resource { get; private set; }

    /// <summary>
    /// Gets the input JSON provided by the user.
    /// </summary>
    protected string Input { get; private set; }

    public BaseDscSubcommand(
        string name,
        string description,
        IOptionFactory optionFactory,
        IResourceProvider resourceProvider)
        : base(name, description)
    {
        OptionFactory = optionFactory;
        ResourceProvider = resourceProvider;

        _resourceOption = OptionFactory.Create<ResourceOption>();
        Options.Add(_resourceOption);

        _inputOption = OptionFactory.Create<InputOption>();
        Options.Add(_inputOption);

        SetAction(CommandHandler);
    }

    /// <summary>
    /// Handles the command by initializing the resource and input, then
    /// calling the internal handler.
    /// </summary>
    /// <param name="parseResult">The parse result containing the command line arguments.</param>
    /// <returns>The exit code of the command.</returns>
    private async Task<int> CommandHandler(ParseResult parseResult)
    {
        // Resource option is validated prior the command handler being invoked.
        Resource = ResourceProvider.GetResource(parseResult.GetValue(_resourceOption));
        Input = parseResult.GetValue(_inputOption);

        // Continue to the specific command handler.
        var result = await CommandHandlerInternalAsync(parseResult);
        return result ? HRESULT.S_OK : HRESULT.E_FAIL;
    }

    /// <summary>
    /// Handles the command logic internally.
    /// </summary>
    /// <param name="parseResult">The parse result containing the command line arguments.</param>
    /// <returns>True if the command was successful; otherwise false.</returns>
    protected abstract Task<bool> CommandHandlerInternalAsync(ParseResult parseResult);
}
