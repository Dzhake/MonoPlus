using System.Text.Json.Serialization;

namespace Monod.ModSystem;

/// <summary>
/// Config file for the <see cref="ModManager"/>, saved to "%Saves%/ModManager/Config.json"
/// </summary>
public class ModManagerConfig
{
    /// <summary>
    /// Whether to load disabled mods at startup.
    /// </summary>
    [JsonInclude] public bool PreloadMods = false;
}