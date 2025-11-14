namespace MonoPlus.AssetsSystem;

/// <summary>
/// Provides methods for loading assets from an external source.
/// </summary>
public interface IAssetLoader
{
    /// <summary>
    /// Manager that uses this <see cref="IAssetLoader"/>.
    /// </summary>
    public AssetManager Manager { get; set; }
    
    /// <summary>
    /// Load all "assets.json" for this <see cref="IAssetLoader"/> asynchronously, can be used for reloading.
    /// </summary>
    public void LoadAssetManifests();
    
    /// <summary>
    /// Total amount of "assets.json" files that are loaded or are loading right now.
    /// </summary>
    public int TotalAssetManifests { get; }
    
    /// <summary>
    /// Amount of currently loaded "assets.json" files.
    /// </summary>
    public int LoadedAssetManifests { get; }

    /// <summary>
    /// Load all assets in the cache asynchronously, <b>without replacing</b> already loaded assets.
    /// </summary>
    public void LoadAssets();

    /// <summary>
    /// Load asset at the specified path in the cache synchronously, <b>without replacing</b> already loaded assets. Useful for quickly loading fonts for the startup loading screen.
    /// </summary>
    public void LoadAsset(string path);

    /// <summary>
    /// Load all assets in the cache asynchronously, replacing already loaded assets.
    /// </summary>
    public void ReloadAssets();
    
    /// <summary>
    /// Total amount of assets that are loaded or are loading right now.
    /// </summary>
    public int TotalAssets { get; }
    
    /// <summary>
    /// Amount of currently loaded assets.
    /// </summary>
    public int LoadedAssets { get; }
    
    /// <summary>
    /// String that will be shown as name of the asset manager.
    /// </summary>
    public string DisplayName { get; }
    
    /// <summary>
    /// Get asset at the specified path from the cache. <see cref="LoadAssets"/> must be called first.
    /// </summary>
    /// <param name="path">Path of the asset in this <see cref="IAssetLoader"/>.</param>
    /// <returns>The asset, or <see langword="null"/> if asset at the specified <paramref cref="path"/> was not found.</returns>
    public object? GetAsset(string path);
}