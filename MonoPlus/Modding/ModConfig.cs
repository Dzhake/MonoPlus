using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace MonoPlus.ModSystem;

/// <summary>
/// Represents serialized configuration for a <see cref="Mod"/>,
/// </summary>
public class ModConfig
{
    /// <summary>
    /// Mod's Id — <see cref="Mod"/>'s unique name and version.
    /// </summary>
    [JsonInclude]
    public required ModId Id;

    /// <summary>
    /// <see cref="File"/> path, relative to mod's directory, to .dll file related to mod.
    /// </summary>
    [JsonInclude]
    public string? AssemblyFile;

    /// <summary>
    /// Dependencies of the mod, which must be loaded for mod to run.
    /// </summary>
    [JsonInclude]
    public List<ModDep>? HardDeps;

    /// <summary>
    /// Dependencies of the mod, which must be loaded for mod to run.
    /// </summary>
    [JsonInclude] public List<ModDep>? SoftDeps;
}
