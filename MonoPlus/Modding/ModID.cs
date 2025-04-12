using System.Text.Json.Serialization;
using Chasm.SemanticVersioning;
using Chasm.SemanticVersioning.Ranges;

namespace MonoPlus.Modding;

/// <summary>
/// Represents ID of <see cref="Mod"/>
/// </summary>
public struct ModID
{
    /// <summary>
    /// Mod's unique name
    /// </summary>
    [JsonInclude]
    public string Name;

    /// <summary>
    /// Mod's version
    /// </summary>
    [JsonInclude]
    public SemanticVersion Version;

    /// <summary>
    /// Check is this <see cref="ModID"/> satisfieds <see cref="dep"/>
    /// </summary>
    /// <param name="dep">Dependency to check</param>
    /// <returns><see langword="true"/> if names are equal, and <see cref="Version"/> is without dep's <see cref="VersionRange"/>, <see langword="false"/> otherwise</returns>
    public bool Matches(ModDep dep)
    {
        return Name == dep.Name && dep.Versions.IsSatisfiedBy(Version);
    }
}
