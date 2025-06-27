using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Serilog;

namespace MonoPlus.AssetsSystem;

/// <summary>
///   <para>Provides a set of static methods to manage asset managers and load assets.</para>
/// </summary>
public static class Assets
{
    private static readonly Dictionary<string, AssetsManager> Managers = new();

    /// <summary>
    ///   <para>Adds the specified asset <paramref name="assetsManager"/> to the global registry under the specified <paramref name="prefix"/>.</para>
    /// </summary>
    /// <param name="assetsManager">The asset manager to register under the specified <paramref name="prefix"/>.</param>
    /// <param name="prefix">The global prefix to register the specified asset <paramref name="assetsManager"/> under.</param>
    /// <exception cref="ArgumentNullException"><paramref name="assetsManager"/> or <paramref name="prefix"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="assetsManager"/> already has a registered prefix, or the specified prefix is already occupied by another asset manager.</exception>
    public static void RegisterAssetManager(AssetsManager assetsManager, string prefix)
    {
        ArgumentNullException.ThrowIfNull(assetsManager);
        ArgumentNullException.ThrowIfNull(prefix);
        if (assetsManager.Prefix is not null)
            throw new ArgumentException("The specified manager already has a registered prefix.", nameof(assetsManager));

        Managers.Add(prefix, assetsManager);
        assetsManager.Prefix = prefix;
        Log.Information("Registered asset manager with prefix: {Prefix}", prefix);
    }
    
    /// <summary>
    ///   <para>Removes the specified asset <paramref name="assetsManager"/> from the global registry.</para>
    /// </summary>
    /// <param name="assetsManager">The asset manager to remove from the global registry.</param>
    /// <returns><see langword="true"/>, if the specified asset <paramref name="assetsManager"/> was successfully removed; otherwise, <see langword="false"/>.</returns>
    public static bool UnRegisterAssetsManager([NotNullWhen(true)] AssetsManager? assetsManager)
    {
        if (assetsManager?.Prefix is { } prefix && Managers.Remove(prefix))
        {
            assetsManager.Prefix = null;
            Log.Information("Unregistered asset manager with prefix: {Prefix}", prefix);
            return true;
        }

        Log.Information("Failed to unregister asset manager: {IAssetsManager}", assetsManager);
        return false;
    }

    /// <summary>
    ///   <para>Loads and returns an asset at the specified <paramref name="fullPath"/>.</para>
    /// </summary>
    /// <typeparam name="T">The type of the asset to load.</typeparam>
    /// <param name="fullPath">A fully qualified path to the asset to get the handle of.</param>
    /// <returns>A value task containing either the loaded asset, or the asset loading task.</returns>
    /// <exception cref="InvalidCastException">The asset at the specified <paramref name="fullPath"/> could not be cast to type <typeparamref name="T"/>.</exception>
    /// <exception cref="ArgumentException">Could not find asset with the specified prefix.</exception>
    [MustUseReturnValue]
    public static T Get<T>(string fullPath)
    {
        SplitPath(fullPath, out var prefix, out var relativePath);

        if (!Managers.TryGetValue(prefix.ToString(), out AssetsManager? manager))
            throw new ArgumentException("Could not find asset with the specified prefix.", nameof(fullPath));
        return manager.Get<T>(relativePath.ToString());
    }

    /// <summary>
    ///   <para>Same as <see cref="Get{T}"/>, but returns <see langword="null"/> if asset was not found, instead of using <see cref="Assets.NotFoundPolicy"/>.</para>
    /// </summary>
    /// <typeparam name="T">The type of the asset to load.</typeparam>
    /// <param name="fullPath">A fully qualified path to the asset to get the handle of.</param>
    /// <returns>A value task containing either the loaded asset, or the asset loading task.</returns>
    /// <exception cref="InvalidCastException">The asset at the specified <paramref name="fullPath"/> could not be cast to type <typeparamref name="T"/>.</exception>
    /// <exception cref="ArgumentException">Could not find asset with the specified prefix.</exception>
    [MustUseReturnValue]
    public static T? GetOrDefault<T>(string fullPath)
    {
        SplitPath(fullPath, out var prefix, out var relativePath);

        if (!Managers.TryGetValue(prefix.ToString(), out AssetsManager? manager))
            throw new ArgumentException("Could not find asset with the specified prefix.", nameof(fullPath));
        return manager.GetOrDefault<T>(relativePath.ToString());
    }

    /// <summary>
    /// Checks if <see cref="AssetsManager"/> with specified <paramref name="prefix"/> is registered.
    /// </summary>
    /// <param name="prefix"><see cref="AssetsManager"/>'s prefix.</param>
    /// <returns>Whether <see cref="AssetsManager"/> with specified <paramref name="prefix"/> is registered.</returns>
    public static bool AssetsManagerRegistered(string prefix) => Managers.ContainsKey(prefix);

    /// <summary>
    /// Splits asset path with prefix info prefix and asset path
    /// </summary>
    /// <param name="query">Path to split.</param>
    /// <param name="prefix">Prefix of asset manager.</param>
    /// <param name="path">Asset path for asset manager.</param>
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
    /// Type of resource program should aim for.
    /// </summary>
    public enum ResourcePriorityType
    {
        /// <summary>
        /// Maximum performance, don't care about memory usage.
        /// </summary>
        Performance, 
        
        /// <summary>
        /// Minimum memory usage, but probably lower performance.
        /// </summary>
        Memory,
    }

    /// <summary>
    /// Whether <see cref="AssetsManager"/>s should prefer maximum performance, or lower memory usage. Used in rare cases, where <see cref="ResourcePriorityType.Performance"/> can use a lot of memory.
    /// </summary>
    public static ResourcePriorityType ResourcePriority = ResourcePriorityType.Performance;

    /// <summary>
    /// Types of action <see cref="AssetsManager"/> will do if the specified asset was not found.
    /// </summary>
    public enum NotFoundPolicyType
    {
        /// <summary>
        /// Throw <see cref="AssetNotFoundException"/>.
        /// </summary>
        Exception,
        
        /// <summary>
        /// Fallback to default asset.
        /// </summary>
        Fallback,
    }

    /// <summary>
    /// Action <see cref="AssetsManager"/> will do if the specified asset was not found.
    /// </summary>
    public static NotFoundPolicyType NotFoundPolicy = NotFoundPolicyType.Fallback;
}
