using System;
using System.Collections.Generic;

namespace MonoPlus.AssetsSystem;

/// <summary>
/// Represents an object which listens to assets being reloaded, and loads the assets again on reload.
/// </summary>
public interface IAssetListener : IDisposable
{
    /// <summary>
    /// Called after <see cref="AssetManager"/> reloads assets, for every <see cref="IAssetListener"/> which was added to <see cref="AssetManager"/> via <see cref="AssetManager.AddListener"/>
    /// </summary>
    /// <param name="oldAssets">Assets before reloading, which you can use to check if your asset was reloaded, <b>or <see langword="null"/> if all assets were reloaded</b></param>
    public void ReloadAssets(HashSet<object>? oldAssets);

    IAssetListener()
    {
        
    }

    void IDisposable.Dispose()
    {
        
    }
}
