using System;
using System.IO;
using System.Text.Json;
using HarmonyLib;

namespace MonoPlus.ModSystem;

/// <summary>
/// Helper class for quickly creating mods with failure problems from an <see cref="Exception"/>.
/// </summary>
public static class FailedToLoadMod
{
    /// <summary>
    /// Creates a new instance of <see cref="Mod"/> with <see cref="Mod.Status"/> set to <see cref="ModStatus.FailedToLoad"/>.
    /// </summary>
    /// <param name="failureReason">Why mod failed to load. Set to <see cref="Mod.FailureReason"/>.</param>
    /// <param name="config">Mod's config. If mod config failed to load, then use <see cref="New(Exception, string)"/>.</param>
    /// <returns>A new instance of <see cref="Mod"/> with <see cref="Mod.Status"/> set to <see cref="ModStatus.FailedToLoad"/>.</returns>
    public static Mod New(Exception failureReason, ModConfig config) =>
        new()
        {
            Status = ModStatus.FailedToLoad,
            FailureReason = failureReason,
            Config = config,
        };

    /// <summary>
    /// Creates a new instance of <see cref="Mod"/> with <see cref="Mod.Status"/> set to <see cref="ModStatus.FailedToLoad"/>.
    /// </summary>
    /// <param name="failureReason">Why mod failed to load. Set to <see cref="Mod.FailureReason"/>.</param>
    /// <param name="name">Mod's name. If mod config was loaded, then use <see cref="New(Exception, ModConfig)"/> instead.</param>
    /// <returns>A new instance of <see cref="Mod"/> with <see cref="Mod.Status"/> set to <see cref="ModStatus.FailedToLoad"/>.</returns>
    public static Mod New(Exception failureReason, string name) =>
        new()
        {
            Status = ModStatus.FailedToLoad,
            FailureReason = failureReason,
            Config = new()
            {
                Id = new(name, new(999,999,999))
            }
        };

    /// <summary>
    /// <see cref="JsonSerializer"/> returned <see langword="null"/> for config.json at the specified <see cref="configPath"/>.
    /// </summary>
    /// <param name="configPath"><see cref="File"/> path of the "config.json".</param>
    /// <param name="modName">Name of mod related to this config. Usually a name of directory where config.json was located.</param>
    /// <returns>A new instance of <see cref="Mod"/> with <see cref="Mod.FailureReason"/> being <i>*summary*</i></returns>
    public static Mod ConfigDeserializeNull(string configPath, string modName) => New(new($"Deserializer returned null for config at: {configPath}"), modName);
    
    /// <summary>
    /// Could not satisfy hard deps of a <see cref="ModConfig"/>.
    /// </summary>
    /// <param name="config">Config of mod which failed to load.</param>
    /// <returns>A new instance of <see cref="Mod"/> with <see cref="Mod.FailureReason"/> being <i>*summary*</i></returns>
    public static Mod HardDepsNotMet(ModConfig config) => New(new($"Could not satisfy hard dependencies of {config.Id.Name}: {config.HardDeps?.Join()}"), config);
    
    
}