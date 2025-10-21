// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Extensions;
using WinGetStudio.Services.DesiredStateConfiguration.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Models.Schemas.ConfigurationV3;

namespace WinGetStudio.ViewModels;

public sealed partial class SetViewModel : ObservableObject
{
    private readonly ILogger _logger;
    private readonly IStringLocalizer _localizer;
    private readonly ObservableCollection<UnitViewModel> _units;

    public ReadOnlyObservableCollection<UnitViewModel> Units { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private partial IDSCFile? OriginalDscFile { get; set; }

    [ObservableProperty]
    private partial IDSCFile? CurrentDscFile { get; set; }

    [ObservableProperty]
    public partial string? Code { get; set; }

    public bool CanSave => OriginalDscFile?.CanSave ?? false;

    public event NotifyCollectionChangedEventHandler? UnitsCollectionChanged
    {
        add => _units.CollectionChanged += value;
        remove => _units.CollectionChanged -= value;
    }

    public SetViewModel(ILogger logger, IStringLocalizer localizer)
    {
        _logger = logger;
        _localizer = localizer;
        _units = [];
        Units = new(_units);
    }

    public async Task UseAsync(IDSCSet dscSet, IDSCFile dscFile)
    {
        Debug.Assert(Units.Count == 0, "Units collection should be empty when initializing from a DSC set.");
        OriginalDscFile = dscFile;

        // Update units and code.
        Code = dscFile?.Content;

        var units = dscSet?.Units ?? [];
        var tasks = units.Select(async unit =>
        {
            UnitViewModel vm = new(_localizer);
            await vm.CopyFromAsync(unit);
            return vm;
        });

        var result = await Task.WhenAll(tasks);
        foreach (var unit in result)
        {
            _units.Add(unit);
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
        await original.CopyFromAsync(updated);
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

    public IDSCFile GetLatestDSCFile()
    {
        if (CurrentDscFile == null)
        {
            if (OriginalDscFile?.FileInfo != null)
            {
                CurrentDscFile = DSCFile.CreateVirtual(OriginalDscFile.FileInfo.FullName, Code);
            }
            else
            {
                CurrentDscFile = DSCFile.CreateVirtual(Code);
            }
        }

        return CurrentDscFile;
    }

    private async Task UpdateConfigurationCodeAsync()
    {
        _logger.LogInformation("Updating configuration code");
        Code = await GenerateConfigurationCodeAsync();
        CurrentDscFile = null;
    }

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

    public async Task SaveAsync()
    {
        var dscFile = GetLatestDSCFile();
        if (dscFile.CanSave)
        {
            await dscFile.SaveAsync(_localizer);
        }
    }

    public async Task SaveAsAsync(string filePath)
    {
        OriginalDscFile = DSCFile.CreateVirtual(filePath, Code);
        await OriginalDscFile.SaveAsync(_localizer);
    }

    partial void OnOriginalDscFileChanged(IDSCFile? oldValue, IDSCFile? newValue)
    {
        // When the original DSC file changes, reset the current DSC file to match it.
        CurrentDscFile = newValue;
    }
}
