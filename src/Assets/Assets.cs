using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;
using Monod.AssetsSystem;
using Serilog;

namespace MonoPlus.AssetsSystem;

/// <summary>
///   <para>Provides a set of static methods to manage asset managers and load assets.</para>
/// </summary>
public static class Assets
{
    /// <summary>
    /// Dictionary, which maps <see cref="File"/> extension <b>(WITH the period)</b> to function which accepts <see cref="FileStream"/> and returns an asset, similar to <see cref="Texture2D.FromStream(GraphicsDevice, Stream)"/>. Use it if you want to load asset of your own format, not supported by MonoPlus. Keep in mind multiply mods might try to add values with same keys to this.
    /// </summary>
    public static readonly Dictionary<string, Func<FileStream, object>> CustomFormats = new();
        
    /// <summary>
    /// All registered <see cref="AssetManager"/>s.
    /// </summary>
    private static readonly Dictionary<string, AssetManager> Managers = new();
    
    /// <summary>
    /// List of <see cref="IAssetLoader"/>s, that are currently reloading some assets. Used to determine whether the game should pause and wait until assets are reloaded.
    /// </summary>
    public static readonly HashSet<IAssetLoader> ReloadingAssetLoaders = new();

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
        if (assetManager.Prefix is not null)
            throw new ArgumentException("The specified manager already has a registered prefix.", nameof(assetManager));

        Managers.Add(prefix, assetManager);
        assetManager.Prefix = prefix;
        Log.Information("Registered asset manager with prefix: {Prefix}", prefix);
    }
    
    /// <summary>
    ///   <para>Removes the specified asset <paramref name="assetsManager"/> from the global registry.</para>
    /// </summary>
    /// <param name="assetsManager">The asset manager to remove from the global registry.</param>
    /// <returns><see langword="true"/>, if the specified asset <paramref name="assetsManager"/> was successfully removed; otherwise, <see langword="false"/>.</returns>
    public static bool UnRegisterAssetsManager([NotNullWhen(true)] AssetManager? assetsManager)
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
    /// Checks if <see cref="AssetManager"/> with specified <paramref name="prefix"/> is registered.
    /// </summary>
    /// <param name="prefix"><see cref="AssetManager"/>'s prefix.</param>
    /// <returns>Whether <see cref="AssetManager"/> with specified <paramref name="prefix"/> is registered.</returns>
    public static bool AssetsManagerRegistered(string prefix) => Managers.ContainsKey(prefix);
    

    /// <summary>
    ///   <para>Loads and returns an asset at the specified <paramref name="fullPath"/>.</para>
    /// </summary>
    /// <typeparam name="T">The type of the asset to load.</typeparam>
    /// <param name="fullPath">A fully qualified path to the asset to get the handle of.</param>
    /// <returns>A value task containing either the loaded asset, or the asset loading task.</returns>
    /// <exception cref="InvalidCastException">The asset at the specified <paramref name="fullPath"/> could not be cast to type <typeparamref name="T"/>.</exception>
    /// <exception cref="ArgumentException">Could not find asset with the specified prefix.</exception>
    [MustUseReturnValue]
    public static T Get<T>(string fullPath, out int? listenerIndex, IAssetListener? listener = null)
    {
        SplitPath(fullPath, out var prefix, out var relativePath);

        if (!Managers.TryGetValue(prefix.ToString(), out AssetManager? manager))
            throw new ArgumentException("Could not find asset with the specified prefix.", nameof(fullPath));
        if (listener is not null)
            listenerIndex = manager.AddListener(listener);
        else
            listenerIndex = null;
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

        if (!Managers.TryGetValue(prefix.ToString(), out AssetManager? manager))
            throw new ArgumentException("Could not find asset with the specified prefix.", nameof(fullPath));
        return manager.GetOrDefault<T>(relativePath.ToString());
    }

    /// <summary>
    /// Splits asset path with prefix info prefix and asset path
    /// </summary>
    /// <param name="query">Path to split.</param>
    /// <param name="prefix">Prefix of asset manager.</param>
    /// <param name="path">Asset path for asset manager.</param>
    public static void SplitPath(ReadOnlySpan<char> query, out ReadOnlySpan<char> prefix, out ReadOnlySpan<char> path)
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
    /// Whether <see cref="AssetManager"/>s should prefer maximum performance, or lower memory usage. Used in rare cases, where <see cref="ResourcePriorityType.Performance"/> can use a lot of memory.
    /// </summary>
    public static ResourcePriorityType ResourcePriority = ResourcePriorityType.Performance;
    
    

    /// <summary>
    /// Types of action <see cref="AssetManager"/> will do if the specified asset was not found.
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
    /// Action <see cref="AssetManager"/> will do if the specified asset was not found.
    /// </summary>
    public static NotFoundPolicyType NotFoundPolicy = NotFoundPolicyType.Fallback;
}
