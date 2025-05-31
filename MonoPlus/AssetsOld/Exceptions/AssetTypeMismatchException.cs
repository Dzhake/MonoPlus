using System;

namespace MonoPlus.AssetsManagement;

/// <summary>
/// The exception that is thrown when received asset's type was not what expected.
/// </summary>
public sealed class AssetTypeMismatchException : Exception
{
    /// <summary>
    /// Gets the asset manager that loaded the asset.
    /// </summary>
    public AssetManager AssetManager { get; }

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
    /// <param name="assetManager">The asset manager that the specified asset could not be found in.</param>
    /// <param name="relativePath">A relative path to the asset that could not be found.</param>
    public AssetTypeMismatchException(AssetManager assetManager, string relativePath, Type expectedType, Type receivedType)
    {
        ArgumentNullException.ThrowIfNull(assetManager);
        ArgumentNullException.ThrowIfNull(relativePath);
        ArgumentNullException.ThrowIfNull(expectedType);
        ArgumentNullException.ThrowIfNull(receivedType);
        AssetManager = assetManager;
        RelativePath = relativePath;
        ExpectedType = expectedType;
        ReceivedType = receivedType;
    }
}