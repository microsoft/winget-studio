// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using Microsoft.Management.Configuration;

namespace WinGetStudio.Services.DesiredStateConfiguration.Models;

internal sealed class DSCSet : IDSCSet
{
    /// <inheritdoc/>
    public Guid InstanceIdentifier { get; }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc />
    public IReadOnlyList<IDSCUnit> Units => UnitsInternal.AsReadOnly();

    /// <summary>
    /// Gets the list of units in this set.
    /// </summary>
    /// <remarks>
    /// This list maintains the concrete type of the objects which is internal
    /// to this service project.
    /// </remarks>
    internal IList<DSCUnit> UnitsInternal { get; }

    /// <summary>
    /// Gets the <see cref="ConfigurationProcessor"/> instance used to process configuration settings.
    /// </summary>
    internal ConfigurationProcessor Processor { get; }

    /// <summary>
    /// Gets the <see cref="ConfigurationSet"/> instance that this object wraps.
    /// </summary>
    internal ConfigurationSet ConfigSet { get; }

    public DSCSet(ConfigurationProcessor processor, ConfigurationSet configSet)
    {
        Processor = processor;
        ConfigSet = configSet;

        // Constructor copies all the required data from the out-of-proc COM
        // objects over to the current process. This ensures that we have this
        // information available even if the out-of-proc COM objects are no
        // longer available (e.g. AppInstaller service is no longer running).
        InstanceIdentifier = configSet.InstanceIdentifier;
        Name = configSet.Name;
        UnitsInternal = configSet.Units.Select(unit => new DSCUnit(unit)).ToList();
    }
}
