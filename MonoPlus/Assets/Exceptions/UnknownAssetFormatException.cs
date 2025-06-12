using System;

namespace MonoPlus.AssetsSystem;

/// <summary>
///     <para>The exception that is thrown when an asset at the specified path is not any known format.</para>
/// </summary>
public sealed class UnknownAssetFormatException : Exception
{
    /// <summary>
    ///   <para>Gets the asset manager that the specified asset found in.</para>
    /// </summary>
    public AssetsManager AssetsManager { get; }
    /// <summary>
    ///   <para>Gets a relative path to the asset.</para>
    /// </summary>
    public string RelativePath { get; }

    /// <inheritdoc/>
    public override string Message => $"Asset '{RelativePath}' in {AssetsManager} is not any known type.";

    /// <summary>
    ///   <para>Initializes a new instance of the <see cref="UnknownAssetFormatException"/> class with the specified <paramref name="assetManager"/> and <paramref name="relativePath"/>.</para>
    /// </summary>
    /// <param name="assetManager">The asset manager that the specified asset could not be found in.</param>
    /// <param name="relativePath">A relative path to the asset that could not be found.</param>
    public UnknownAssetFormatException(AssetsManager assetManager, string relativePath)
    {
        ArgumentNullException.ThrowIfNull(assetManager);
        ArgumentNullException.ThrowIfNull(relativePath);
        AssetsManager = assetManager;
        RelativePath = relativePath;
    }
}
