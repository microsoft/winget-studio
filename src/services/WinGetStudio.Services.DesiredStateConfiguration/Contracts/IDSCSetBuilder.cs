// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Contracts;

public interface IDSCSetBuilder
{
    // Target file path for when the set is saved to a file
    public string TargetFilePath { get; set; }

    // List of the units in the set
    public IReadOnlyList<IDSCUnit> Units { get; }

    /// <summary>
    /// Adds a new unit to the collection of editable DSC units.
    /// </summary>
    /// <remarks>This method appends the specified unit to the collection.</remarks>
    /// <param name="unit">The <see cref="EditableDSCUnit"/> to add. Cannot be <see langword="null"/>.</param>
    public void AddUnit(EditableDSCUnit unit);

    /// <summary>
    /// Removes the Unit with the same InstanceIdentifier as <paramref name="unit"/>
    /// </summary>
    /// <param name="unit">Unit to remove</param>
    public void RemoveUnit(EditableDSCUnit unit);

    /// <summary>
    /// Removes all units from the collection.
    /// </summary>
    /// <remarks>After calling this method, the collection will be empty. This operation does not raise any
    /// events or perform additional actions beyond clearing the collection.</remarks>
    public void ClearUnits();

    /// <summary>
    /// Updates the Unit in the set with the same InstanceIdentifier as <paramref name="unit"/> to match <paramref name="unit"/>
    /// </summary>
    /// <param name="unit">Unit to update</param>
    public void UpdateUnit(EditableDSCUnit unit);

    /// <summary>
    /// Loads the state of <paramref name="dscSet"/> into this object
    /// </summary>
    /// <param name="dscSet">The DSCSet to load from</param>
    public void ImportSet(IDSCSet dscSet);

    /// <summary>
    /// Checks whether Units is Empty
    /// </summary>
    /// <returns>False if Units.Count > 0, True otherwise</returns>
    public bool IsEmpty();

    /// <summary>
    /// Creates a new DSCSet object from the current state of this class
    /// </summary>
    /// <returns>The created DSCSet</returns>
    public Task<IDSCSet> BuildAsync();

    /// <summary>
    /// Checks if the current state matches that of the DSC Configuration file passed in
    /// </summary>
    /// <remarks><paramref name="yaml"/> must be valid yaml and a valid DSC Configuration file</remarks>
    /// <param name="yaml">The string to compare to</param>
    /// <returns>True if the current state matches that of the input string, False otherwise</returns>
    public Task<bool> EqualsYamlAsync(string yaml);

    /// <summary>
    /// Generates a string containing the DSC Configuration file corresponding to the current state
    /// </summary>
    /// <returns>A string containing the current state of this object converted into yaml</returns>
    public Task<string> ConvertToYamlAsync();
}
