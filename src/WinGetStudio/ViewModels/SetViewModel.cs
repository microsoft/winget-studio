// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Extensions;
using WinGetStudio.Services.DesiredStateConfiguration.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Models.Schemas.ConfigurationV3;

namespace WinGetStudio.ViewModels;

public sealed partial class SetViewModel : ObservableObject
{
    private readonly ILogger _logger;
    private readonly ObservableCollection<UnitViewModel> _units;

    public ReadOnlyObservableCollection<UnitViewModel> Units { get; }

    public IDSCFile? OriginalDscFile { get; private set; }

    [ObservableProperty]
    public partial string? Code { get; set; }

    private IDSCFile? CurrentDscFile { get; set; }

    public event NotifyCollectionChangedEventHandler? UnitsCollectionChanged
    {
        add => _units.CollectionChanged += value;
        remove => _units.CollectionChanged -= value;
    }

    public SetViewModel(ILogger logger)
    {
        _logger = logger;
        _units = [];
        Units = new(_units);
    }

    public void Use(IDSCSet dscSet, IDSCFile dscFile)
    {
        Debug.Assert(Units.Count == 0, "Units collection should be empty when initializing from a DSC set.");
        OriginalDscFile = dscFile;
        CurrentDscFile = dscFile;

        // Update units and code.
        Code = dscFile?.Content;
        foreach (var unit in dscSet?.Units ?? [])
        {
            _units.Add(new(unit));
        }

        // Resolve dependencies between units.
        ResolveDependencies();
    }

    public async Task AddAsync(UnitViewModel unit)
    {
        _units.Insert(0, unit);
        unit.ResolveDependencies(this);
        await UpdateConfigurationCodeAsync();
    }

    public async Task RemoveAsync(UnitViewModel unit)
    {
        _units.Remove(unit);
        ResolveDependencies();
        await UpdateConfigurationCodeAsync();
    }

    public async Task UpdateAsync(UnitViewModel original, UnitViewModel updated)
    {
        updated.Validate();
        original.CopyFrom(updated);
        await UpdateConfigurationCodeAsync();
    }

    public void ResolveDependencies()
    {
        _logger.LogInformation("Resolving dependencies between configuration units");
        foreach (var unit in Units)
        {
            unit.ResolveDependencies(this);
        }
    }

    public async Task UpdateConfigurationCodeAsync()
    {
        _logger.LogInformation("Updating configuration code");
        Code = await GenerateConfigurationCodeAsync();
        CurrentDscFile = null;
    }

    public IDSCFile GetLatestDSCFile() => CurrentDscFile ?? DSCFile.CreateVirtual(Code);

    private Task<string> GenerateConfigurationCodeAsync()
    {
        return Task.Run(() =>
        {
            var config = new ConfigurationV3();
            config.AddWinGetMetadata();
            foreach (var unit in Units)
            {
                var unitConfig = unit.ToConfigurationV3();
                config.Resources.AddRange(unitConfig.Resources);
            }

            return config.ToYaml();
        });
    }
}
