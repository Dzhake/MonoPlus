using System;

namespace MonoPlus.AssetsSystem;

/// <summary>
/// Represents an object which listens to assets being reloaded, and loads the assets again on reload.
/// </summary>
public interface IAssetListener : IDisposable
{
    /// <summary>
    /// Called after <see cref="AssetManager"/> reloads assets, for every <see cref="IAssetListener"/> which was added to <see cref="AssetManager"/> via <see cref="AssetManager.AddListener"/>
    /// </summary>
    public void LoadAssets(bool reloading);
}
