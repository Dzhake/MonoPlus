using System;
using Monod.AssetsSystem;

namespace Monod.AssetsSystem;

/// <summary>
/// The exception that is thrown when <see cref="assetManager"/> could not find fallback for an asset.
/// </summary>
public sealed class AssetFallbackNotFoundException : Exception
{
    /// <summary>
    /// Gets the asset manager that loaded the asset.
    /// </summary>
    private readonly AssetManager assetManager;

    /// <summary>
    /// Gets a relative path to the asset.
    /// </summary>
    private readonly Type FallbackType;

    /// <inheritdoc/>
    public override string Message => $"\"{assetManager}\" could not find fallback for type: {FallbackType}";

    /// <summary>
    ///   <para>Initializes a new instance of the <see cref="AssetFallbackNotFoundException"/> with the specified <see cref="assetManager"/> and <see cref="FallbackType"/>.</para>
    /// </summary>
    /// <param name="assetManager">The asset manager that could not find fallback for the specified <paramref name="fallbackType"/></param>
    /// <param name="fallbackType">Type of the fallback that asset manager tried to find.</param>
    public AssetFallbackNotFoundException(AssetManager assetManager, Type fallbackType)
    {
        ArgumentNullException.ThrowIfNull(assetManager);
        ArgumentNullException.ThrowIfNull(fallbackType);
        this.assetManager = assetManager;
        FallbackType = fallbackType;
    }
}