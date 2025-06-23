namespace MonoPlus.ModSystem;

/// <summary>
/// Represents a set of virtual methods which <see cref="ModManager"/> calls for each mod.
/// </summary>
public abstract class ModListener
{
    /// <summary>
    /// Mod related to this <see cref="ModListener"/>.
    /// </summary>
    public required Mod mod;

    /// <summary>
    /// Whether <see cref="PostInitialized"/> was called for this listener. Used to determine whether to call it.
    /// </summary>
    public bool PostInitialized;

    /// <summary>
    /// Called when mod's assembly is loaded.
    /// </summary>
    public virtual void Initialize()
    {
    }

    /// <summary>
    /// Called after all mods are loaded.
    /// </summary>
    public virtual void PostInitialize()
    {
    }

    /// <summary>
    /// Called every frame, in Game's Update.
    /// </summary>
    public virtual void Update()
    {
    }
}