using System.IO;

namespace MonoPlus.Modding;

/// <summary>
/// Represents info for delayed <see cref="ModConfig"/>
/// </summary>
public struct DelayedConfigInfo(ModConfig config, string modDir)
{
    /// <summary>
    /// Delayed <see cref="ModConfig"/>
    /// </summary>
    public ModConfig Config = config;

    /// <summary>
    /// <see cref="Directory"/> where <see cref="Config"/> is located
    /// </summary>
    public string ModDir = modDir;
}
