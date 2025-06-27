﻿using System;

 namespace MonoPlus.AssetsSystem;

/// <summary>
///   <para>The exception that is thrown when an asset at the specified path is not found in cache.</para>
/// </summary>
public sealed class AssetNotFoundException : Exception
{
    /// <summary>
    ///   <para>Gets the asset manager that the specified asset could not be found in.</para>
    /// </summary>
    public AssetsManager AssetManager { get; }

    /// <summary>
    ///   <para>Gets a relative path to the asset that could not be found.</para>
    /// </summary>
    public string RelativePath { get; }

    /// <inheritdoc/>
    public override string Message => $"Asset '{RelativePath}' could not be found in {AssetManager}.";

    /// <summary>
    ///   <para>Initializes a new instance of the <see cref="AssetNotFoundException"/> class with the specified <paramref name="assetsManager"/> and <paramref name="relativePath"/>.</para>
    /// </summary>
    /// <param name="assetsManager">The asset manager that the specified asset could not be found in.</param>
    /// <param name="relativePath">A relative path to the asset that could not be found.</param>
    public AssetNotFoundException(AssetsManager assetsManager, string relativePath)
    {
        ArgumentNullException.ThrowIfNull(assetsManager);
        ArgumentNullException.ThrowIfNull(relativePath);
        AssetManager = assetsManager;
        RelativePath = relativePath;
    }
}