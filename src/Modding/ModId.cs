using System;
using System.Text.Json.Serialization;
using Chasm.SemanticVersioning;
using Chasm.SemanticVersioning.Ranges;

namespace Monod.ModSystem;

/// <summary>
/// Represents Id of a <see cref="Mod"/>.
/// </summary>
public readonly struct ModId : IEquatable<ModId>
{
    /// <summary>
    /// Mod's unique name.
    /// </summary>
    [JsonInclude]
    public readonly string Name;

    /// <summary>
    /// Mod's version.
    /// </summary>
    [JsonInclude]
    public readonly SemanticVersion Version;


    /// <summary>
    /// Initializes a new instance of <see cref="ModId"/> with specified <see cref="Name"/> and <see cref="Version"/>.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="version"></param>
    [JsonConstructor]
    public ModId(string name, SemanticVersion version)
    {
        Name = name;
        Version = version;
    }

    /// <summary>
    /// Check is this <see cref="ModId"/> satisfieds the specified <paramref name="dep"/>.
    /// </summary>
    /// <param name="dep">Dependency to check.</param>
    /// <returns>Whether names are equal, and <paramref name="dep.Versions"/> include <see cref="Version"/>.</returns>
    public bool Matches(ModDep dep)
    {
        return Name == dep.Name && dep.Versions.IsSatisfiedBy(Version);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is ModId other && Name == other.Name && Version.Equals(other.Version);
    }

    /// <inheritdoc/>  
    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Version);
    }

    
    /// <summary>
    /// Whether two specified <see cref="ModId"/>s are equal.
    /// </summary>
    /// <param name="left">One <see cref="ModId"/>.</param>
    /// <param name="right">Other <see cref="ModId"/>.</param>
    /// <returns>Whether two specified <see cref="ModId"/>s are equal.</returns>
    public static bool operator ==(ModId left, ModId right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Whether two specified <see cref="ModId"/>s are not equal.
    /// </summary>
    /// <param name="left">One <see cref="ModId"/>.</param>
    /// <param name="right">Other <see cref="ModId"/>.</param>
    /// <returns>Whether two specified <see cref="ModId"/>s are not equal.</returns>
    public static bool operator !=(ModId left, ModId right)
    {
        return !(left == right);
    }

    /// <inheritdoc/> 
    public override string ToString()
    {
        return $"{Name} v{Version}";
    }

    /// <inheritdoc />
    public bool Equals(ModId other) => string.Equals(Name, other.Name, StringComparison.InvariantCultureIgnoreCase) && Version.Equals(other.Version);
}
