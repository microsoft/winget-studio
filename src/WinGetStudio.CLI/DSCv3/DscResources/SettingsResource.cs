// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.CLI.DSCv3.DscResources;

internal sealed partial class SettingsResource : BaseResource
{
    public const string ResourceName = "settings";

    public SettingsResource()
        : base(ResourceName)
    {
    }

    public override bool ExportState(string input) => throw new System.NotImplementedException();

    public override bool GetState(string input) => throw new System.NotImplementedException();

    public override bool SetState(string input) => throw new System.NotImplementedException();

    public override bool TestState(string input) => throw new System.NotImplementedException();

    public override bool Manifest(string outputDir) => throw new System.NotImplementedException();

    public override bool Schema() => throw new System.NotImplementedException();
}
