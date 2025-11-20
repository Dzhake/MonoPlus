using Monod.AssetsSystem;

namespace MonodExamples.AssetsSystem;

/// <summary>
/// Example of using <see cref="Assets"/> for learning.
/// </summary>
public class AssetUserExample : IDisposable
{
    /// <summary>
    /// Full path of the asset.
    /// </summary>
    public readonly string AssetPath; // It's fine both with and without "readonly" in this case.
    
    /// <summary>
    /// The asset.
    /// </summary>
    public required string Asset; // "required" because otherwise Rider diagnoses this field as not being set in ctor. Could as well just suppress the warning.
    
    /// <summary>
    /// Whether <see cref="Dispose"/> has been called.
    /// </summary>
    protected bool disposed;
    
    /// <summary>
    /// Create a new <see cref="AssetUserExample"/> using the specified <paramref name="assetPath"/>.
    /// </summary>
    /// <param name="assetPath"></param>
    public AssetUserExample(string assetPath)
    {
        AssetPath = assetPath; // Store the path, so we can use it later.
        Assets.OnReload += LoadAssets; // Subscribe "LoadAssets" to "Assets.OnReload", so when assets reload "LoadAssets" is called. 
        LoadAssets(); // Load assets (after setting the "AssetPath" !).
    }

    /// <summary>
    /// Get needed assets from assets cache to this object.
    /// </summary>
    public void LoadAssets()
    {
        Asset = Assets.Get<string>(AssetPath); // Get the asset at the specified path. Path is "manager:asset", where "manager" is name of the asset manager, "asset" is relative path to the asset in that asset manager, and ":" is a literal symbol in the string.
    }
    
    /// <summary>
    /// Unsubscribe from "Assets.OnReload". Derived class should override "Dispose" to unsubscribe themself too, if needed.
    /// </summary>
    protected void UnsubscribeSelf()
    {
        Assets.OnReload -= LoadAssets;
    }

    /// <summary>
    /// Dispose this object.
    /// </summary>
    public void Dispose()
    {
        if (disposed) return; // Don't unsubscribe the object twice.
        disposed = true;
        UnsubscribeSelf(); // Don't forget to unsubscribe.
        GC.SuppressFinalize(this); //"so derived classes don't need to override dispose if they add finalizers" warning/suggestion
    }
}