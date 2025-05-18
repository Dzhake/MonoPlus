using System.Reflection;
using HarmonyLib;
using MonoPlus.AssetsManagement;
using Serilog;

namespace MonoPlus.Modding;

/// <summary>
/// Contains info about mod which should not be serialized into <see cref="ModConfig"/>, and callback methods
/// </summary>
public class Mod
{
    /// <summary>
    /// Read from config.json <see cref="ModConfig"/> for <see langword="this"/> <see cref="Mod"/>
    /// </summary>
    public ModConfig Config = null!;

    /// <summary>
    /// Used for assets used by <see langword="this"/> <see cref="Mod"/>
    /// </summary>
    public AssetManager? Assets;

    /// <summary>
    /// Used to manage and unload <see cref="Assembly"/> loaded by <see langword="this"/> <see cref="Mod"/>
    /// </summary>
    public ModAssemblyLoadContext? AssemblyContext;

    /// <summary>
    /// Used to <see cref="Log"/> <see cref="Mod"/>-specific info by using <see cref="Mod.Config"/>'s <see cref="ModId.Name"/> in lines logged by this logger.
    /// </summary>
    public ILogger LoggerInstance = null!;

    /// <summary>
    /// Used to patch methods, patches made with this will be correctly reloaded when reloading mod assembly. Is null before <see cref="AssignConfig"/> called.
    /// </summary>
    public Harmony HarmonyInstance = null!;

    /// <summary>
    /// Ties <paramref name="config"/> with <see langword="this"/>, and sets <see cref="LoggerInstance"/> and <see cref="HarmonyInstance"/> based on <paramref name="config"/>
    /// </summary>
    /// <param name="config"></param>
    public void AssignConfig(ModConfig config)
    {
        Config = config;
        Config.mod = this;
        LoggerInstance = Log.ForContext("Mod", Config.Id.Name);
        HarmonyInstance = new(Config.Id.Name);
    }

    /// <summary>
    /// Called when mod's assembly is loaded
    /// </summary>
    public virtual void PreInitialize() { }

    /// <summary>
    /// Called after all mods are loaded
    /// </summary>
    public virtual void Initialize() { }

    /// <summary>
    /// Called every frame
    /// </summary>
    public virtual void Update() { }
}
