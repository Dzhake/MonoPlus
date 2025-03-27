﻿using System;
 using System.Collections.Generic;
 using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace MonoPlus.Assets;

/// <summary>
///   <para>Represents an asset manager, that loads and caches various assets.</para>
/// </summary>
public abstract class AssetManager : IDisposable
{
    internal string? registeredPrefix;
    /// <summary>
    ///   <para>The asset manager's cache of loaded assets.</para>
    /// </summary>
    protected StringKeyedDictionary<AssetCacheEntry> _cache = new();
    /// <summary>
    ///   <para>The asset manager's dictionary of listeners which should be notified when asset is loaded/refreshed.</para>
    /// </summary>
    protected StringKeyedDictionary<Action> _listeners = new();
    /// <summary>
    ///   <para>The asset manager's cache reader-writer lock.</para>
    /// </summary>
    protected readonly ReaderWriterLockSlim _cacheRwl = new();
    /// <summary>
    ///   <para>Determines whether this asset manager has been disposed of.</para>
    /// </summary>
    protected int disposed;

    /// <summary>
    ///   <para>Gets the asset manager's display name.</para>
    /// </summary>
    public abstract string DisplayName { get; }

    /// <summary>
    ///   <para>Returns the string representation of this asset manager: its display name along with the registered prefix.</para>
    /// </summary>
    /// <returns>The string representation of this asset manager.</returns>
    public override string ToString()
        => registeredPrefix is null ? $"{DisplayName} (external)" : $"{DisplayName} ({registeredPrefix}:/*)";

    /// <summary>
    ///   <para>Releases all resources used by the asset manager.</para>
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) == 1) return;
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    /// <summary>
    ///   <para>Releases the unmanaged resources used by the asset manager and optionally releases the managed resources.</para>
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Assets.UnRegisterAssetManager(this);
            _cache = null!;
        }
    }

    /// <summary>
    ///   <para>Returns a new asset, loaded from the specified path, or <see langword="null"/> if the specified asset is not found.</para>
    /// </summary>
    /// <param name="assetPath">A relative path to the asset to load.</param>
    /// <returns>A new asset, loaded from the specified path, or <see langword="null"/> if it is not found.</returns>
    protected abstract ValueTask<object?> LoadNewAssetAsync(string assetPath);

    public void LoadIntoCache(string path)
    {
        if (path.IndexOf('\\') >= 0) path = path.ToString().Replace('\\', '/');

        try
        {
            _cacheRwl.EnterWriteLock();

            // Start loading the new asset asynchronously,
            // Add the AssetEntry for this asset in the cache
            _cache.Add(path, new(LoadNewAssetAsync(path)));
            NotifyListenersOnAssetRefresh(path);
        }
        finally
        {
            _cacheRwl.ExitWriteLock();
        }
    }

    private ValueTask<object?> LoadAssetCore(ReadOnlySpan<char> path, bool startLoading)
    {
        ObjectDisposedException.ThrowIf(disposed != 0, this);

        // Normalize the path
        if (path.IndexOf('\\') >= 0) path = path.ToString().Replace('\\', '/');
        string pathString = path.ToString();

        try
        {
            _cacheRwl.EnterReadLock();
            // Try to read the asset from cache
            if (_cache.TryGetValue(path.ToString(), out AssetCacheEntry entry))
                return entry.GetValueAsync();

            if (!startLoading)
            {
                _cacheRwl.ExitReadLock();
                return default;
            }

            ValueTask<object?> loading = LoadNewAssetAsync(pathString);

            // Add the AssetEntry for this asset in the cache
            _cache.Add(pathString, new(loading));

            NotifyListenersOnAssetRefresh(pathString);
            _cacheRwl.ExitReadLock();
            return loading;

        }
        finally
        {
            _cacheRwl.ExitReadLock();
        }
        
    }

    private ValueTask<T?> LoadCore<T>(ReadOnlySpan<char> path,  bool startLoading)
        => LoadAssetCore(path,startLoading).Transform((asset => (T?)asset));

    [Pure] public ValueTask<T> LoadAsync<T>(string path)
        => LoadCore<T>(path, true)!;
    [Pure] public ValueTask<T> LoadAsync<T>(ReadOnlySpan<char> path)
        => LoadCore<T>(path, true)!;
    protected void RefreshAsset(ReadOnlySpan<char> relativePath)
    {
        try
        {
            _cacheRwl.EnterWriteLock();
            if (_cache.TryGetKey(relativePath, out string? pathString))
            {
                // Start reloading the asset asynchronously
                ValueTask<object?> loading = LoadNewAssetAsync(pathString);

                // Set the AssetEntry for this asset in the cache
                _cache[pathString] = new(loading);
                
                // TODO: what about stale values? How do we do that? Especially relevant for errors
            }
        }
        finally
        {
            _cacheRwl.ExitWriteLock();
        }
    }

    protected readonly struct AssetCacheEntry
    {
        // _value is either the loaded asset (object) or its loading task (Task<object>).
        private readonly object? _value;

        public AssetCacheEntry(ValueTask<object?> task)
            => _value = task.IsCompletedSuccessfully ? task.Result : task.AsTask();
        public AssetCacheEntry(ValueTask<object?> task, object? previousValue)
            => _value = task.IsCompletedSuccessfully ? task.Result : new StaleWhileReload(task, previousValue);

        public ValueTask<object?> GetValueAsync()
        {
            object? obj = _value;

            if (obj is Task<object?> task) return new(task);

            return new(obj is StaleWhileReload stale ? stale.Value : obj);
        }

        private sealed class StaleWhileReload(ValueTask<object?> reloadTask, object? staleValue)
        {
            public object? Value => reloadTask.IsCompletedSuccessfully ? reloadTask.Result : staleValue;
        }
    }

    public virtual void PreloadAssetsAsync() {}

    protected void NotifyListenersOnAssetRefresh(string assetPath)
    {

    }
}