// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

namespace WinGetStudio.Services.DesiredStateConfiguration.Models;
public class EditableDSCUnit : IDSCUnit
{
    public string Type { get; set; } = "";

    public string Id { get; set; } = "";

    public Guid InstanceId { get; } = Guid.NewGuid();

    public string Description { get; set; } = "";

    public string Intent { get; set; } = "Apply";

    public string ModuleName { get; set; } = "";

    public IList<string> Dependencies { get; set; } = new List<string>();

    public IList<KeyValuePair<string, object>> Settings { get; set; } = new List<KeyValuePair<string, object>>();

    public IList<KeyValuePair<string, string>> Metadata { get; set; } = new List<KeyValuePair<string, string>>();

    public bool RequiresElevation { get; set; } = false;

    public Task<IDSCUnitDetails> GetDetailsAsync()
    {
        return Task.FromResult<IDSCUnitDetails>(new DSCUnitDetails(ModuleName));
    }
}
