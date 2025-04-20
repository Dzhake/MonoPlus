using System;
using System.Text.Json.Serialization;
using Chasm.SemanticVersioning;
using Chasm.SemanticVersioning.Ranges;

namespace MonoPlus.Modding;

/// <summary>
/// Represents ID of <see cref="Mod"/>
/// </summary>
public readonly struct ModID
{
    /// <summary>
    /// Mod's unique name
    /// </summary>
    [JsonInclude]
    public readonly string Name;

    /// <summary>
    /// Mod's version
    /// </summary>
    [JsonInclude]
    public readonly SemanticVersion Version;


    /// <summary>
    /// Instances a new <see cref="ModID"/> with specified <see cref="Name"/> and <see cref="Version"/>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="version"></param>
    [JsonConstructor]
    public ModID(string name, SemanticVersion version)
    {
        Name = name;
        Version = version;
    }

    /// <summary>
    /// Check is this <see cref="ModID"/> satisfieds <paramref name="dep"/>
    /// </summary>
    /// <param name="dep">Dependency to check</param>
    /// <returns><see langword="true"/> if names are equal, and <see cref="Version"/> is without dep's <see cref="VersionRange"/>, <see langword="false"/> otherwise</returns>
    public bool Matches(ModDep dep)
    {
        return Name == dep.Name && dep.Versions.IsSatisfiedBy(Version);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is ModID other && Name == other.Name && Version.Equals(other.Version);
    }

    /// <inheritdoc/>  
    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Version);
    }

    /// <summary>
    /// Checks if two <see cref="ModID"/>s <see cref="Equals"/>
    /// </summary>
    public static bool operator ==(ModID left, ModID right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Checks whether two <see cref="ModID"/>s don't <see cref="Equals"/>
    /// </summary>
    public static bool operator !=(ModID left, ModID right)
    {
        return !(left == right);
    }

    /// <inheritdoc/> 
    public override string ToString()
    {
        return $"{Name} v{Version}";
    }
}
