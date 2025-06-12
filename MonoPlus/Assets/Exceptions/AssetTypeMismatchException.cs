using System;

namespace MonoPlus.AssetsSystem;

/// <summary>
/// The exception that is thrown when received asset's type was not what expected.
/// </summary>
public sealed class AssetTypeMismatchException : Exception
{
    /// <summary>
    /// Gets the asset manager that loaded the asset.
    /// </summary>
    public AssetsManager AssetManager { get; }

    /// <summary>
    /// Gets a relative path to the asset.
    /// </summary>
    public string RelativePath { get; }

    /// <summary>
    /// Expected type of the asset.
    /// </summary>
    public Type ExpectedType { get; }

    /// <summary>
    /// Tyoe if the asset which was received.
    /// </summary>
    public Type ReceivedType { get; }

    /// <inheritdoc/>
    public override string Message => $"Mismatch asset type at path {RelativePath} in manager {AssetManager}: expected {ExpectedType}, but received {ReceivedType}";

    /// <summary>
    ///   <para>Initializes a new instance of the <see cref="AssetNotFoundException"/> class with the specified <paramref name="assetManager"/> and <paramref name="relativePath"/>.</para>
    /// </summary>
    /// <param name="assetsManager">The asset manager that the specified asset could not be found in.</param>
    /// <param name="relativePath">A relative path to the asset that could not be found.</param>
    public AssetTypeMismatchException(AssetsManager assetsManager, string relativePath, Type expectedType, Type receivedType)
    {
        ArgumentNullException.ThrowIfNull(assetsManager);
        ArgumentNullException.ThrowIfNull(relativePath);
        ArgumentNullException.ThrowIfNull(expectedType);
        ArgumentNullException.ThrowIfNull(receivedType);
        AssetManager = assetsManager;
        RelativePath = relativePath;
        ExpectedType = expectedType;
        ReceivedType = receivedType;
    }
}