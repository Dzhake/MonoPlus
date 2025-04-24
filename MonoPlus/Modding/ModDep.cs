using System;
using System.Text.Json.Serialization;
using Chasm.SemanticVersioning.Ranges;

namespace MonoPlus.Modding;

/// <summary>
/// Represents info about <see cref="Mod"/>'s dependency
/// </summary>
public readonly struct ModDep
{
    /// <summary>
    /// Dependency's unique name
    /// </summary>
    [JsonInclude]
    public readonly string Name;

    /// <summary>
    /// <see cref="VersionRange"/> of acceptable versions
    /// </summary>
    [JsonInclude]
    public readonly VersionRange Versions;

    /// <summary>
    /// Instances a new <see cref="ModDep"/> with specified <see cref="Name"/> and <see cref="Versions"/>
    /// </summary>
    /// <param name="name">Dependency's unique name</param>
    /// <param name="versions"><see cref="VersionRange"/> of acceptable versions</param>
    [JsonConstructor]
    public ModDep(string name, VersionRange versions)
    {
        Name = name;
        Versions = versions;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is ModDep other && Name == other.Name && Versions.Equals(other.Versions);
    }

    /// <inheritdoc/>  
    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Versions);
    }

    /// <summary>
    /// Checks if two <see cref="ModId"/>s <see cref="Equals"/>
    /// </summary>
    public static bool operator ==(ModDep left, ModDep right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Checks whether two <see cref="ModId"/>s don't <see cref="Equals"/>
    /// </summary>
    public static bool operator !=(ModDep left, ModDep right)
    {
        return !(left == right);
    }

    /// <inheritdoc/> 
    public override string ToString()
    {
        return $"{Name} v{Versions}";
    }
}
