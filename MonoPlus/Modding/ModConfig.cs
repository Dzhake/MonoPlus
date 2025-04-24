using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace MonoPlus.Modding;

/// <summary>
/// Represents configuration data for <see cref="Mod"/>
/// </summary>
public class ModConfig
{
    /// <summary>
    /// Mod's Id — <see cref="Mod"/>'s unique name and version
    /// </summary>
    [JsonInclude]
    public required ModId Id;

    /// <summary>
    /// <see cref="File"/> path, relative to mod's directory, to .dll file related to mod
    /// </summary>
    [JsonInclude]
    public string? AssemblyFile;

    /// <summary>
    /// Dependencies of the mod
    /// </summary>
    [JsonInclude]
    public List<ModDep>? Dependencies;

    /// <summary>
    /// <see cref="Mod"/> related to this config
    /// </summary>
    public Mod? mod;

    /// <summary>
    /// <see cref="Directory"/> path where this config was located.
    /// </summary>
    public string ModDirectory = null!;
}
