// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.Operations.Contracts;
using WinGetStudio.Services.Operations.Extensions;

namespace WinGetStudio.Models.Operations;

public sealed partial class SaveDSCFileOperation : IOperation<OperationResult<bool>>
{
    private readonly ILogger<SaveDSCFileOperation> _logger;
    private readonly IStringLocalizer _localizer;
    private readonly IDSCFile _dscFile;

    public SaveDSCFileOperation(ILogger<SaveDSCFileOperation> logger, IStringLocalizer localizer, IDSCFile dscFile)
    {
        _logger = logger;
        _localizer = localizer;
        _dscFile = dscFile;
    }

    /// <inheritdoc/>
    public async Task<OperationResult<bool>> ExecuteAsync(IOperationContext context)
    {
        try
        {
            await _dscFile.SaveAsync();
            context.Success(props => props with { Message = _localizer["PreviewFile_SavedSuccessfully"] });
            return new() { Result = true };
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "Failed to save DSC file due to null or empty file path.");
            context.Fail(props => props with { Message = _localizer["File_PathCannotBeNullOrEmpty"] });
            return new() { Error = ex };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while saving the DSC file.");
            context.Fail(props => props with { Message = _localizer["PreviewFile_SaveFailed", ex.Message] });
            return new() { Error = ex };
        }
    }
}
