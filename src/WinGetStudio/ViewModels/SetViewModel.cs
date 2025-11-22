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

public delegate SetViewModel SetViewModelFactory();

public sealed partial class SetViewModel : ObservableObject
{
    private readonly ILogger<SetViewModel> _logger;
    private readonly IStringLocalizer<SetViewModel> _localizer;
    private readonly UnitViewModelFactory _unitFactory;
    private readonly ObservableCollection<UnitViewModel> _units;

    public ReadOnlyObservableCollection<UnitViewModel> Units { get; }

    public string? Code => CodeInternal;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    [NotifyPropertyChangedFor(nameof(FilePath))]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    private partial IDSCFile? OriginalDscFile { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    private partial IDSCFile? CurrentDscFile { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    [NotifyPropertyChangedFor(nameof(Code))]
    private partial string? CodeInternal { get; set; }

    public bool CanSave => OriginalDscFile?.CanSave ?? false;

    public bool HasUnsavedChanges => !OriginalDscFile?.Equals(CurrentDscFile) ?? true;

    public string? FilePath => OriginalDscFile?.FileInfo?.FullName;

    public event NotifyCollectionChangedEventHandler? UnitsCollectionChanged
    {
        add => _units.CollectionChanged += value;
        remove => _units.CollectionChanged -= value;
    }

    public SetViewModel(ILogger<SetViewModel> logger, IStringLocalizer<SetViewModel> localizer, UnitViewModelFactory unitFactory)
    {
        _logger = logger;
        _localizer = localizer;
        _unitFactory = unitFactory;
        _units = [];
        Units = new(_units);
    }

    public async Task UseAsync(IDSCSet dscSet, IDSCFile dscFile)
    {
        Debug.Assert(Units.Count == 0, "Units collection should be empty when initializing from a DSC set.");
        OriginalDscFile = dscFile;

        // Update units and code.
        CodeInternal = dscFile?.Content;

        var units = dscSet?.Units ?? [];
        var tasks = units.Select(async unit =>
        {
            var vm = _unitFactory();
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
        CodeInternal = await GenerateConfigurationCodeAsync();
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
            await SaveInternalAsync(dscFile);
        }
    }

    public async Task SaveAsAsync(string filePath)
    {
        var dscFile = DSCFile.CreateVirtual(filePath, Code);
        await SaveInternalAsync(dscFile);
    }

    partial void OnOriginalDscFileChanged(IDSCFile? oldValue, IDSCFile? newValue)
    {
        // When the original DSC file changes, reset the current DSC file to match it.
        CurrentDscFile = newValue;
    }

    private async Task SaveInternalAsync(IDSCFile dscFile)
    {
        Debug.Assert(dscFile.CanSave, $"DSC file should be savable before calling {nameof(SaveInternalAsync)}.");
        OriginalDscFile = dscFile;
        await dscFile.SaveAsync(_localizer);
        OnPropertyChanged(nameof(HasUnsavedChanges));
    }
}
