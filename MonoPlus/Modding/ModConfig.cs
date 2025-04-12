using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MonoPlus.Modding;

/// <summary>
/// Represents configuration data for <see cref="Mod"/>
/// </summary>
public class ModConfig
{
    /// <summary>
    /// Mod's ID — It's unique name and version
    /// </summary>
    [JsonInclude]
    public required ModID ID;

    /// <summary>
    /// File path, relative to mod's directory, to .dll file related to mod
    /// </summary>
    [JsonInclude]
    public string? AssemblyFile;

    /// <summary>
    /// Dependencies of the mod
    /// </summary>
    [JsonInclude]
    public List<ModDep>? Dependencies;
}
