using System.Reflection;
using MonoPlus.AssetsManagment;
using Serilog;

namespace MonoPlus.Modding;

/// <summary>
/// Contains info about mod which should not be serialized into <see cref="ModConfig"/>, and callback methods
/// </summary>
public class Mod
{

    public ModConfig Config;
    public AssetManager? Assets;
    public Assembly? assembly;
    public ILogger Logger;

    public Mod(ModConfig config)
    {
        Config = config;
        Logger = Log.ForContext("Module", Config.ID.Name);
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
