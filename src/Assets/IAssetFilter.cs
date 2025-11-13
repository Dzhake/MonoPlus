namespace MonoPlus.AssetsSystem;

/// <summary>
/// Allows <see cref="AssetManager"/> to not load some specific asset based on its path. Can be shared between multiply <see cref="AssetManager"/>s.
/// </summary>
public interface IAssetFilter
{
    /// <summary>
    /// Whether asset at the specified path should be loaded. If <see langword="false"/>, then <see cref="AssetManager"/> should completely ignore the asset.
    /// </summary>
    /// <param name="path">Path of the asset.</param>
    /// <returns>Whether asset at the specified path should be loaded.</returns>
    public bool ShouldLoad(string path);
}