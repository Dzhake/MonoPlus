using System;
using System.IO;

namespace MonoPlus.AssetsSystem;

/// <summary>
///   <para>The exception that is thrown when <see cref="FileSystemAssetManager"/> found more than asset with same path and name but different extensions</para>
/// </summary>
public sealed class DuplicateAssetException : Exception
{
    /// <summary>
    ///   <para>Gets the asset manager that the specified asset could not be found in.</para>
    /// </summary>
    public AssetsManager assetsManager { get; }
    /// <summary>
    ///   <para>Gets a relative path to the asset that could not be found.</para>
    /// </summary>
    public string AssetExtensions { get; }

    /// <summary>
    ///     <para>Path where <see cref="FileSystemAssetManager"/> tried to find asset</para>
    /// </summary>
    public string RootPath { get; }

    /// <inheritdoc/>
    public override string Message => $"{assetsManager} found multiply assets at path {RootPath} with extensions: {AssetExtensions}.";

    /// <summary>
    ///     <para>Initializes a new instance of the <see cref="DuplicateAssetException"/> class.</para>
    /// </summary>
    /// <param name="assetsManager"><see cref="AssetsManager"/> that found duplicated assets.</param>
    /// <param name="rootPath"><see cref="Directory"/> where assets were found</param>
    /// <param name="assetExtensions"><see cref="Array"/> of file extensions of duplicate assets.</param>
    public DuplicateAssetException(AssetsManager assetsManager, string rootPath, string[] assetExtensions)
    {
        ArgumentNullException.ThrowIfNull(assetsManager);
        ArgumentNullException.ThrowIfNull(rootPath);
        ArgumentNullException.ThrowIfNull(assetExtensions);
        this.assetsManager = assetsManager;
        RootPath = rootPath;
        AssetExtensions = string.Join(',', assetExtensions);
    }
}
