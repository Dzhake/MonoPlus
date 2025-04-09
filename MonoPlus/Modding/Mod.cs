using System.Reflection;
using MonoPlus.Assets;

namespace MonoPlus.Modding;

public class Mod
{
    public required ModConfig Config;
    public AssetManager? Assets;
    public required Assembly assembly;

    /// <summary>
    /// Called when mod's assembly is loaded
    /// </summary>
    public virtual void Initialize()
    {

    }

    /// <summary>
    /// Called after all mods are loaded
    /// </summary>
    public virtual void PostInitialize()
    {

    }

    /// <summary>
    /// Called every frame
    /// </summary>
    public virtual void Update()
    {

    }
}
