﻿using System;
 using System.Collections.Generic;
 using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Serilog;

namespace MonoPlus.AssetsManagement;

/// <summary>
///   <para>Provides a set of static methods to manage asset managers and load assets.</para>
/// </summary>
public static class Assets
{
    private static readonly Dictionary<string, AssetManager> managers = new();

    /// <summary>
    ///   <para>Adds the specified asset <paramref name="assetManager"/> to the global registry under the specified <paramref name="prefix"/>.</para>
    /// </summary>
    /// <param name="assetManager">The asset manager to register under the specified <paramref name="prefix"/>.</param>
    /// <param name="prefix">The global prefix to register the specified asset <paramref name="assetManager"/> under.</param>
    /// <exception cref="ArgumentNullException"><paramref name="assetManager"/> or <paramref name="prefix"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="assetManager"/> already has a registered prefix, or the specified prefix is already occupied by another asset manager.</exception>
    public static void RegisterAssetManager(AssetManager assetManager, string prefix)
    {
        ArgumentNullException.ThrowIfNull(assetManager);
        ArgumentNullException.ThrowIfNull(prefix);
        if (assetManager.registeredPrefix is not null)
            throw new ArgumentException("The specified manager already has a registered prefix.", nameof(assetManager));

        managers.Add(prefix, assetManager);
        assetManager.registeredPrefix = prefix;
        Log.Information("Registered asset manager with prefix: {Prefix}", prefix);
    }
    /// <summary>
    ///   <para>Removes the specified asset <paramref name="assetManager"/> from the global registry.</para>
    /// </summary>
    /// <param name="assetManager">The asset manager to remove from the global registry.</param>
    /// <returns><see langword="true"/>, if the specified asset <paramref name="assetManager"/> was successfully removed; otherwise, <see langword="false"/>.</returns>
    public static bool UnRegisterAssetManager([NotNullWhen(true)] AssetManager? assetManager)
    {
        if (assetManager?.registeredPrefix is { } prefix && managers.Remove(prefix))
        {
            assetManager.registeredPrefix = null;
            Log.Information("Unregistered asset manager with prefix: {Prefix}", prefix);
            return true;
        }

        Log.Information("Failed to unregister asset manager: {AssetManager}", assetManager);
        return false;
    }

    /// <summary>
    ///   <para>Loads and returns an asset at the specified <paramref name="fullPath"/>.</para>
    /// </summary>
    /// <typeparam name="T">The type of the asset to load.</typeparam>
    /// <param name="fullPath">A fully qualified path to the asset to get the handle of.</param>
    /// <returns>A value task containing either the loaded asset, or the asset loading task.</returns>
    /// <exception cref="InvalidCastException">The asset at the specified <paramref name="fullPath"/> could not be cast to type <typeparamref name="T"/>.</exception>
    /// <exception cref="ArgumentException">Could not find specified asset manager prefix</exception>
    [MustUseReturnValue]
    public static ValueTask<T> LoadAsync<T>(ReadOnlySpan<char> fullPath)
    {
        SplitPath(fullPath, out var prefix, out var relativePath);

        if (!managers.TryGetValue(prefix.ToString(), out AssetManager? manager))
            throw new ArgumentException("Could not find specified asset manager prefix.", nameof(fullPath));

        return manager.LoadAsync<T>(relativePath);
    }

    /// <inheritdoc cref="LoadAsync{T}(ReadOnlySpan{char})"/>
    /// <exception cref="ArgumentNullException"><paramref name="fullPath"/> is <see langword="null"/>.</exception>
    [MustUseReturnValue]
    public static ValueTask<T> LoadAsync<T>(string fullPath)
    {
        ArgumentNullException.ThrowIfNull(fullPath);
        return LoadAsync<T>(fullPath.AsSpan());
    }

    /// <summary>
    /// Checks if <see cref="AssetManager"/> with specified <paramref name="prefix"/> is registered
    /// </summary>
    /// <param name="prefix"><see cref="AssetManager"/>'s prefix</param>
    /// <returns>Whether <see cref="AssetManager"/> with specified <paramref name="prefix"/> is registered</returns>
    public static bool AssetManagerRegistered(string prefix) => managers.ContainsKey(prefix);


    /// <summary>
    /// Splits asset path with prefix info prefix and asset path
    /// </summary>
    /// <param name="query">Path to split</param>
    /// <param name="prefix">Prefix of asset manager</param>
    /// <param name="path">Asset path for asset manager</param>
    private static void SplitPath(ReadOnlySpan<char> query, out ReadOnlySpan<char> prefix, out ReadOnlySpan<char> path)
    {
        int separatorIndex = query.IndexOf(":/");
        if (separatorIndex == -1)
        {
            //no prefix, only <path>
            prefix = default;
            path = query;
            return;
        }
        // <prefix> ':/' <path>
        prefix = query[..separatorIndex];
        path = query[(separatorIndex + 2)..];
    }


    /// <summary>
    /// Types of priority for code
    /// </summary>
    public enum ResourcePriorityType
    {
        /// <summary>
        /// Maximum preformance, don't care about memory usage
        /// </summary>
        Performance, 
        
        /// <summary>
        /// Minimum memory usage, may affect performance
        /// </summary>
        Memory,
    }

    /// <summary>
    /// <para>Represents should asset managers prefer performance or lower memory usage, used in rare cases.</para>
    /// </summary>
    public static ResourcePriorityType ResourcePriority = ResourcePriorityType.Performance;
}