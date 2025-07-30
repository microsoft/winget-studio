// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

namespace WinGetStudio.Models;
public class ConfigurationPageParameter
{
    public IDSCSet DSCSet { get; set; } = null;
    public bool ResetDSCSet { get; set; } = false;
}
