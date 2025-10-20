// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Management.Configuration;

namespace WinGetStudio.Models;

public sealed partial class UnitSecurityContext
{
    /// <summary>
    /// Gets all security contexts.
    /// </summary>
    public static IReadOnlyList<UnitSecurityContext> All { get; } = [..Enum.GetValues<SecurityContext>().Select(c => new UnitSecurityContext(c.ToString(), c))];

    /// <summary>
    /// Gets the default security context.
    /// </summary>
    public static UnitSecurityContext Default => FromEnum(SecurityContext.Current);

    /// <summary>
    /// Gets the name of the security context.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the security context enum value.
    /// </summary>
    public SecurityContext SecurityContext { get; }

    /// <summary>
    /// Gets the value of the security context.
    /// </summary>
    public string Value => Name.ToLowerInvariant();

    /// <summary>
    /// Gets the security context from the enum value.
    /// </summary>
    /// <param name="securityContext">The security context enum value.</param>
    /// <returns>The corresponding UnitSecurityContext.</returns>
    public static UnitSecurityContext FromEnum(SecurityContext securityContext) => All.First(c => c.SecurityContext == securityContext);

    private UnitSecurityContext(string name, SecurityContext securityContext)
    {
        Name = name;
        SecurityContext = securityContext;
    }
}
