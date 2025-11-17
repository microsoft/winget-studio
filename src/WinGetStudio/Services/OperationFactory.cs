// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Models.Operations;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

namespace WinGetStudio.Services;

public sealed partial class OperationFactory : IOperationFactory
{
    private readonly IDSC _dsc;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IStringLocalizer _localizer;

    public OperationFactory(IDSC dsc, ILoggerFactory loggerFactory, IStringLocalizer localizer)
    {
        _dsc = dsc;
        _loggerFactory = loggerFactory;
        _localizer = localizer;
    }

    /// <inheritdoc/>
    public GetUnitOperation CreateGetUnitOperation(IDSCFile dscFile)
    {
        var logger = _loggerFactory.CreateLogger<GetUnitOperation>();
        return new GetUnitOperation(logger, _localizer, _dsc, dscFile);
    }

    /// <inheritdoc/>
    public SetUnitOperation CreateSetUnitOperation(IDSCFile dscFile)
    {
        var logger = _loggerFactory.CreateLogger<SetUnitOperation>();
        return new SetUnitOperation(logger, _localizer, _dsc, dscFile);
    }

    /// <inheritdoc/>
    public TestUnitOperation CreateTestUnitOperation(IDSCFile dscFile)
    {
        var logger = _loggerFactory.CreateLogger<TestUnitOperation>();
        return new TestUnitOperation(logger, _localizer, _dsc, dscFile);
    }

    /// <inheritdoc/>
    public ValidateSetOperation CreateValidateSetOperation(IDSCFile dscFile)
    {
        var logger = _loggerFactory.CreateLogger<ValidateSetOperation>();
        return new ValidateSetOperation(logger, _localizer, _dsc, dscFile);
    }

    /// <inheritdoc/>
    public ApplySetOperation CreateApplySetOperation(IDSCSet dscSet, IProgress<IDSCSetChangeData> progress)
    {
        var logger = _loggerFactory.CreateLogger<ApplySetOperation>();
        return new ApplySetOperation(logger, _localizer, _dsc, dscSet, progress);
    }

    /// <inheritdoc/>
    public TestSetOperation CreateTestSetOperation(IDSCFile dscFile)
    {
        var logger = _loggerFactory.CreateLogger<TestSetOperation>();
        return new TestSetOperation(logger, _localizer, _dsc, dscFile);
    }

    /// <inheritdoc/>
    public OpenSetOperation CreateOpenSetOperation(IDSCFile dscFile)
    {
        var logger = _loggerFactory.CreateLogger<OpenSetOperation>();
        return new OpenSetOperation(logger, _localizer, _dsc, dscFile);
    }
}
