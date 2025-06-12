using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MonoPlus.Utils;
using MonoPlus.Utils.Collections;
using Serilog;

namespace MonoPlus.AssetsSystem;

/// <summary>
/// Class for loading, storing, caching, and accessing various assets.
/// </summary>
public abstract class AssetsManager : IDisposable
{
    /// <summary>
    /// Cache of the assets.
    /// </summary>
    private Dictionary<string, object> cache = new();

    /// <summary>
    /// List of objects which listen to assets being reloaded, and load the assets again on reload.
    /// </summary>
    private readonly IndexedList<IAssetListener> listeners = new();

    /// <summary>
    ///   <para>Determines whether this asset manager has been disposed.</para>
    /// </summary>
    protected bool disposed;

    /// <summary>
    /// Used for <see cref="cache"/> thread-safety.
    /// </summary>
    private readonly ReaderWriterLockSlim cacheRwl = new();

    /// <summary>
    /// Amount of assets has loaded.
    /// </summary>
    public int LoadedAmount;

    /// <summary>
    /// Amount of assets <see cref="AssetsManager"/> is currently loading, or has loaded earlier.
    /// </summary>
    public int TotalAmount { get; protected set; }

    /// <summary>
    /// Whether the <see cref="AssetsManager"/> is loading some assets, and is not yet ready to be used.
    /// </summary>
    public bool Loading => LoadedAmount < TotalAmount;

    /// <summary>
    /// Whether the <see cref="AssetsManager"/> should reload assets, by itself or via <see cref="ReloadAllAssets"/>.
    /// </summary>
    public bool Reload = true;

    /// <summary>
    /// Unique prefix for this <see cref="AssetsManager"/>, which should be used for <see cref="Assets.Get{T}"/>
    /// </summary>
    public string? Prefix = null;

    /// <summary>
    ///   <para>Gets the asset manager's display name.</para>
    /// </summary>
    protected abstract string DisplayName { get; }
    
    /// <summary>
    ///   <para>Returns the string representation of this asset manager: its display name along with the registered prefix.</para>
    /// </summary>
    /// <returns>The string representation of this asset manager.</returns>
    public override string ToString()
        => Prefix is null ? $"{DisplayName} (no prefix)" : $"{DisplayName} ({Prefix}:/)";

    /// <summary>
    /// Returns asset at the specified <paramref name="path"/>.
    /// </summary>
    /// <typeparam name="T"><see cref="Type"/> of the asset to return. Throws an exception if types of asset in memory and requested types don't match.</typeparam>
    /// <param name="path">Path of the asset in this <see cref="AssetsManager"/></param>
    /// <returns>Asset at the specified <paramref name="path"/></returns>
    public T Get<T>(string path)
    {
        try
        {
            cacheRwl.EnterReadLock();
            if (!cache.TryGetValue(path, out object? asset))
                throw new AssetNotFoundException(this, path);
            if (asset is T castedAsset) return castedAsset;
            throw new AssetTypeMismatchException(this, path, typeof(T), asset.GetType());
        }
        finally
        {
            cacheRwl.ExitReadLock();
        }
    }

    /// <summary>
    /// Reload all assets loaded by this <see cref="AssetsManager"/>.
    /// </summary>
    public void ReloadAllAssets()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (!Reload) return;
        
        try
        {
            cacheRwl.EnterWriteLock();
            cache.Clear();
        }
        finally
        {
            cacheRwl.ExitWriteLock();
        }

        LoadAssets();
        InvokeListeners(null);
    }
    
    

    /// <summary>
    /// Loads all assets related to this manager, e.g. all files from it's directory for file-based all manager.
    /// </summary>
    public void LoadAssets()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        LoadedAmount = 0;
        Log.Information("{AssetManager} is loading assets", this);
        string[] paths = GetAllAssetsPaths();
        TotalAmount = paths.Length;
        foreach (string path in paths)
            MainThread.Add(LoadIntoCacheAsync(path));
    }

    /// <summary>
    /// Returns path of every asset this <see cref="AssetsManager"/> can load. Those paths can then be used in <see cref="LoadIntoCacheAsync"/> to load all possible assets into the cache.
    /// </summary>
    /// <returns>Path of every asset this <see cref="AssetsManager"/> can load</returns>
    protected abstract string[] GetAllAssetsPaths();
    
    /// <summary>
    /// Loads asset into cache using <see cref="LoadNewAssetAsync"/>, ignoring previous asset at that path.
    /// </summary>
    /// <param name="path">Path of the asset in this <see cref="AssetsManager"/></param>
    private async Task LoadIntoCacheAsync(string path)
    {
        try
        {
            object asset = await LoadNewAssetAsync(path);
            cacheRwl.EnterWriteLock();
            cache[path] = asset;
        }
        finally
        {
            cacheRwl.ExitWriteLock();
            Interlocked.Add(ref LoadedAmount, 1);
            Log.Verbose("Loaded asset at path {Prefix}:/{Path} into cache", Prefix, path);
        }
    }

    
    /// <summary>
    /// Reloads asset at specified path and invokes listeners if old asset is found in cache.
    /// </summary>
    /// <param name="path">Path of the asset in this <see cref="AssetsManager"/></param>
    /// <exception cref="AssetNotFoundException">Could not load new asset at the specified path.</exception>
    public void ReloadAsset(string path)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (!Reload) return;
        MainThread.Add(ReloadAssetAsync(path));
    }
    
    /// <summary>
    /// Loads asset into cache using <see cref="LoadNewAssetAsync"/>, and invokes listeners if cache already had asset at the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="path">Path of the asset in this <see cref="AssetsManager"/></param>
    private async Task ReloadAssetAsync(string path)
    {
        try
        {
            object asset = await LoadNewAssetAsync(path);

            //store old asset, replace, invoke listeners.
            cacheRwl.EnterWriteLock();
            cache.TryGetValue(path, out object? oldAsset);

            cache[path] = asset;

            if (oldAsset is null) return;

            object[] oldAssets = [oldAsset];
            InvokeListeners(oldAssets);
        }
        finally
        {
            cacheRwl.ExitWriteLock();
            Interlocked.Add(ref LoadedAmount, 1);
            Log.Verbose("Reloaded asset at path {Prefix}:/{Path}.", Prefix, path);
        }
    }

    /// <summary>
    /// Loads the asset without using cache. Useful for first load and reloading.
    /// </summary>
    /// <param name="path">Path of the asset in this <see cref="AssetsManager"/></param>
    /// <returns>Just loaded asset.</returns>
    protected abstract Task<object> LoadNewAssetAsync(string path);

    /// <summary>
    /// Adds the specified <paramref name="listener"/> to the <see cref="IAssetListener"/>'s listeners.
    /// </summary>
    /// <param name="listener">Listener to add.</param>
    /// <returns>Indexed of the listener, for use with <see cref="RemoveListener"/>.</returns>
    public int AddListener(IAssetListener listener)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        listeners.Add(listener, out int index);
        return index;
    }

    
    
    /// <summary>
    /// Removes listener at specified index.
    /// </summary>
    /// <param name="index">Index of the listener to remove.</param>
    public void RemoveListener(int index)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        listeners.RemoveAt(index);
    }

    /// <summary>
    /// Invokes all listeners with the specified <paramref name="oldAssets"/> param.
    /// </summary>
    /// <param name="oldAssets">Assets before reloading, which you can use to check if your asset was reloaded, <b>or <see langword="null"/> if all assets were reloaded</b></param>
    private void InvokeListeners(object[]? oldAssets)
    {
        foreach (IAssetListener assetListener in listeners)
            assetListener.ReloadAssets(oldAssets);
    }


    /// <inheritdoc/> 
    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, true)) return;
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///   <para>Releases the unmanaged resources used by the asset manager and optionally releases the managed resources.</para>
    /// </summary>
    /// <param name="disposing">Whether to release managed resources too.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;
        Assets.UnRegisterAssetManager(this);
        cache = null!;
    }

    ~AssetsManager()
    {
        Dispose(false);
    }
}