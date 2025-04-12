using System.Text.Json.Serialization;
using Chasm.SemanticVersioning.Ranges;

namespace MonoPlus.Modding;

/// <summary>
/// Represents info about <see cref="Mod"/>'s dependency
/// </summary>
public struct ModDep
{
    /// <summary>
    /// Dependency's unique name
    /// </summary>
    [JsonInclude]
    public string Name;

    /// <summary>
    /// <see cref="VersionRange"/> of acceptable versions
    /// </summary>
    [JsonInclude]
    public VersionRange Versions;
}
