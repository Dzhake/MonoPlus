using System;
using System.Reflection;
using HarmonyLib;
using Monod.ModSystem;
using Monod.AssetsSystem;
using Serilog;

namespace Monod.ModdingNew;

/// <summary>
/// Represents a <see cref="Mod"/> with an assembly, assets, and <see cref="ModListener"/>.
/// </summary>
public sealed class Mod
{
    /// <summary>
    /// Current status of the mod.
    /// </summary>
    public ModStatus Status = ModStatus.Enabled;
    
    /// <summary>
    /// Types of content the mod has.
    /// </summary>
    public ModContentType ContentType = ModContentType.None;

    /// <summary>
    /// Why mod failed to load. Not null if <see cref="Status"/> is <see cref="ModStatus.FailedToLoad"/>.
    /// </summary>
    public Exception? FailureReason;
    
    /// <summary>
    /// Read from config.json <see cref="ModConfig"/> for <see langword="this"/> <see cref="Mod"/>.
    /// </summary>
    public ModConfig Config = null!;

    /// <summary>
    /// <see cref="Directory"/> path where this config was located.
    /// </summary>
    public string Directory = null!;

    /// <summary>
    /// Used for assets used by <see langword="this"/> <see cref="Mod"/>.
    /// </summary>
    public AssetManager? Assets;
    
    /// <summary>
    /// Used to manage and unload <see cref="Assembly"/> loaded by <see langword="this"/> <see cref="Mod"/>.
    /// </summary>
    public ModAssemblyLoadContext? AssemblyContext;

    /// <summary>
    /// Used to <see cref="Log"/> <see cref="Mod"/>-specific info by using <see cref="Mod.Config"/>'s <see cref="ModId.Name"/> in lines logged by this logger.
    /// </summary>
    public ILogger? LoggerInstance;

    /// <summary>
    /// Used to patch methods, patches made with this will be correctly reloaded when reloading mod assembly. Is null before <see cref="AssignConfig"/> called.
    /// </summary>
    public Harmony? HarmonyInstance;

    /// <summary>
    /// Listener for the mod, which contains virtual methods called by <see cref="ModManager"/>.
    /// </summary>
    public ModListener? Listener;

    /// <summary>
    /// Ties <paramref name="config"/> with <see langword="this"/>, and sets <see cref="LoggerInstance"/> and <see cref="HarmonyInstance"/> based on <paramref name="config"/>
    /// </summary>
    /// <param name="config">Config to assign to this <see cref="Mod"/>.</param>
    public void AssignConfig(ModConfig config)
    {
        Config = config;
        string name = Config.Id.Name;
        LoggerInstance = Log.ForContext("Mod", name);
        HarmonyInstance = new(name);
    }
}
