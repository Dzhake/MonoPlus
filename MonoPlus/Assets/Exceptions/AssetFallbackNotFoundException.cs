using System;

namespace MonoPlus.AssetsSystem;

/// <summary>
/// The exception that is thrown when <see cref="AssetsManager"/> could not find fallback for an asset.
/// </summary>
public sealed class AssetFallbackNotFoundException : Exception
{
    /// <summary>
    /// Gets the asset manager that loaded the asset.
    /// </summary>
    private readonly AssetsManager AssetsManager;

    /// <summary>
    /// Gets a relative path to the asset.
    /// </summary>
    private readonly Type FallbackType;

    /// <inheritdoc/>
    public override string Message => $"\"{AssetsManager}\" could not find fallback for type: {FallbackType}";

    /// <summary>
    ///   <para>Initializes a new instance of the <see cref="AssetFallbackNotFoundException"/> with the specified <see cref="AssetsManager"/> and <see cref="FallbackType"/>.</para>
    /// </summary>
    /// <param name="assetsManager">The asset manager that could not find fallback for the specified <paramref name="fallbackType"/></param>
    /// <param name="fallbackType">Type of the fallback that asset manager tried to find.</param>
    public AssetFallbackNotFoundException(AssetsManager assetsManager, Type fallbackType)
    {
        ArgumentNullException.ThrowIfNull(assetsManager);
        ArgumentNullException.ThrowIfNull(fallbackType);
        AssetsManager = assetsManager;
        FallbackType = fallbackType;
    }
}