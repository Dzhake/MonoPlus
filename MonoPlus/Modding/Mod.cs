using System.Reflection;
using MonoPlus.Assets;
using Serilog;

namespace MonoPlus.Modding;

public class Mod
{
    public ModConfig Config;
    public AssetManager? Assets;
    public Assembly? assembly;
    public ILogger Logger;

    public Mod(ModConfig config)
    {
        Config = config;
        Logger = Log.ForContext("Module", Config.Name);
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
