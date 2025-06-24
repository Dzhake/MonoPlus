using System;
using System.Collections.Generic;
using System.IO;
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
    /// <exception cref="AssetTypeMismatchException"><typeparamref name="T"/> does not match type of the loaded asset.</exception>
    public T Get<T>(string path)
    {
        try
        {
            cacheRwl.EnterReadLock();
            if (!cache.TryGetValue(path, out object? asset))
                return Assets.NotFoundPolicy switch
                {
                    Assets.NotFoundPolicyType.Exception => throw new AssetNotFoundException(this, path),
                    Assets.NotFoundPolicyType.Fallback => GetDefault<T>() ?? throw new AssetFallbackNotFoundException(this, typeof(T)),
                    _ => throw new IndexOutOfRangeException($"{nameof(Assets)}.{nameof(Assets.NotFoundPolicy)} was not any known type: {Assets.NotFoundPolicy}")
                };
            if (asset is T castedAsset) return castedAsset;
            throw new AssetTypeMismatchException(this, path, typeof(T), asset.GetType());
        }
        finally
        {
            cacheRwl.ExitReadLock();
        }
    }

    /// <summary>
    /// Same as <see cref="Get{T}"/>, but returns <see langword="null"/> if asset was not found, instead of using <see cref="Assets.NotFoundPolicy"/>.
    /// </summary>
    /// <typeparam name="T"><see cref="Type"/> of the asset to return. Throws an exception if types of asset in memory and requested types don't match.</typeparam>
    /// <param name="path">Path of the asset in this <see cref="AssetsManager"/>.</param>
    /// <returns>Asset at the specified <paramref name="path"/>, or <see langword="null"/> if not found.</returns>
    /// <exception cref="AssetTypeMismatchException"><typeparamref name="T"/> does not match type of the loaded asset.</exception>
    public T? GetOrDefault<T>(string path)
    {
        try
        {
            cacheRwl.EnterReadLock();
            if (!cache.TryGetValue(path, out object? asset))
                return default;
            if (asset is T castedAsset) return castedAsset;
            throw new AssetTypeMismatchException(this, path, typeof(T), asset.GetType());
        }
        finally
        {
            cacheRwl.ExitReadLock();
        }
    }
    
    /// <summary>
    /// Returns default (fallback) asset with the specified <typeparamref name="T"/> for asset that you could not be found.
    /// </summary>
    /// <typeparam name="T">Type of the fallback.</typeparam>
    /// <returns>Fallback asset for that type.</returns>
    /// <exception cref="AssetFallbackNotFoundException">Thrown if asset fallback was not found, or could not be loaded.</exception>
    public static T? GetDefault<T>()
    {
        T fallback = default; //TODO
        return fallback;
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
    /// Loads all assets related to this manager, e.g. all files from its directory for file-based all manager.
    /// </summary>
    public void LoadAssets()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        LoadedAmount = 0;
        Log.Information("{AssetManager} is loading assets", this);
        string[] assetPaths = GetAllAssetsPaths();
        TotalAmount = assetPaths.Length;
        foreach (string assetPath in assetPaths)
            MainThread.Add(Task.Run(async () => await LoadIntoCacheAsync(Path.GetFileNameWithoutExtension(assetPath))));
    }

    /// <summary>
    /// Returns path of every asset this <see cref="AssetsManager"/> can load. Those paths can then be used in <see cref="LoadIntoCacheAsync"/> to load all possible assets into the cache.
    /// </summary>
    /// <returns>Path of every asset this <see cref="AssetsManager"/> can load</returns>
    protected abstract string[] GetAllAssetsPaths();
    
    /// <summary>
    /// Loads asset into cache using <see cref="LoadNewAssetAsync"/>, ignoring previous asset at that path.
    /// </summary>
    /// <param name="assetPath">Path of the asset in this <see cref="AssetsManager"/></param>
    private async Task LoadIntoCacheAsync(string assetPath)
    {
        bool enteredLock = false;
        try
        {
            object? asset = await LoadNewAssetAsync(assetPath);
            cacheRwl.EnterWriteLock();
            enteredLock = true;
            cache[assetPath] = asset!; // Possible null reference assignment. Not an error, if asset was not found then null will be stored, and Get() will return GetDefault() or will crash (based on Assets.NotFoundPolicy). If we store GetDefault(), then if reference to the asset (the path) is changed, cache will have unused value. That's a memory leak.
        }
        finally
        {
            if (enteredLock) cacheRwl.ExitWriteLock();
            Interlocked.Add(ref LoadedAmount, 1);
            Log.Verbose("Loaded asset at path \"{Prefix:l}:/{Path:l}\" into cache", Prefix, assetPath);
        }
    }

    
    /// <summary>
    /// Reloads asset at specified path and invokes listeners if old asset is found in cache.
    /// </summary>
    /// <param name="assetPath">Path of the asset in this <see cref="AssetsManager"/></param>
    /// <exception cref="AssetNotFoundException">Could not load new asset at the specified path.</exception>
    public void ReloadAsset(string assetPath)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (!Reload) return;
        MainThread.Add(ReloadAssetAsync(assetPath));
    }
    
    /// <summary>
    /// Loads asset into cache using <see cref="LoadNewAssetAsync"/>, and invokes listeners if cache already had asset at the specified <paramref name="assetPath"/>.
    /// </summary>
    /// <param name="assetPath">Path of the asset in this <see cref="AssetsManager"/></param>
    private async Task ReloadAssetAsync(string assetPath)
    {
        try
        {
            object? asset = await LoadNewAssetAsync(assetPath);

            //store old asset, replace, invoke listeners.
            cacheRwl.EnterWriteLock();
            cache.TryGetValue(assetPath, out object? oldAsset);

            cache[assetPath] = asset!; //See same line in LoadIntoCacheAsync() for more info about why we suppress the warning here.

            if (oldAsset is null) return;

            object[] oldAssets = [oldAsset];
            InvokeListeners(oldAssets);
        }
        finally
        {
            cacheRwl.ExitWriteLock();
            Interlocked.Add(ref LoadedAmount, 1);
            Log.Verbose("Reloaded asset at path {Prefix}:/{Path}.", Prefix, assetPath);
        }
    }

    /// <summary>
    /// Loads the asset without using cache. Useful for first load and reloading.
    /// </summary>
    /// <param name="assetPath">Path of the asset in this <see cref="AssetsManager"/></param>
    /// <returns>Just loaded asset.</returns>
    protected abstract Task<object?> LoadNewAssetAsync(string assetPath);

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
        Assets.UnRegisterAssetsManager(this);
        if (!disposing) return;
        cache = null!;
    }

    /// <summary>
    /// Deconstructor for <see cref="AssetsManager"/>, which doesn't dispose managed resources.
    /// </summary>
    ~AssetsManager()
    {
        Dispose(false);
    }
}