using System;
using MonoPlus.AssetsSystem;

namespace MonoPlus.ModSystem;

/// <summary>
/// Represents types of content mod can have.
/// </summary>
[Flags]
public enum ModContentType
{
    /// <summary>
    /// Mod doesn't have any content.
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Assets loaded by <see cref="AssetsManager"/>. <see cref="Mod.Assets"/> is not null if mod has this.
    /// </summary>
    Assets = 1,

    /// <summary>
    /// Assembly for the mod. <see cref="Mod.AssemblyContext"/>, <see cref="Mod.LoggerInstance"/>, <see cref="Mod.HarmonyInstance"/> are not null if mod has this.
    /// </summary>
    Code = 1 << 1,
}