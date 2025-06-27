namespace MonoPlus.AssetsSystem;

/// <summary>
/// Represents an object which listens to assets being reloaded, and loads the assets again on reload.
/// </summary>
public interface IAssetListener
{
    /// <summary>
    /// Called after <see cref="AssetsManager"/> reloads assets, for every <see cref="IAssetListener"/> which was added to <see cref="AssetsManager"/> via <see cref="AssetsManager.AddListener"/>
    /// </summary>
    /// <param name="oldAssets">Assets before reloading, which you can use to check if your asset was reloaded, <b>or <see langword="null"/> if all assets were reloaded</b></param>
    public void ReloadAssets(object[]? oldAssets);
}
