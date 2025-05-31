﻿using System;
 using System.Collections.Generic;
 using System.Linq;
 using System.Threading;
using System.Threading.Tasks;
using MonoPlus.Utils;

namespace MonoPlus.AssetsManagement;

/// <summary>
///   <para>Represents an asset manager, that loads and caches various assets.</para>
/// </summary>
public abstract class AssetManager : IDisposable
{
    /// <summary>
    /// Prefix of <see langword="this"/> <see cref="AssetManager"/> for <see cref="Assets"/>.
    /// </summary>
    public string? RegisteredPrefix;

    /// <summary>
    ///   <para>The asset manager's cache of loaded assets.</para>
    /// </summary>
    protected StringKeyedDictionary<object> _cache = new();

    /// <summary>
    ///   <para>The asset manager's dictionary of listeners which should be notified when asset is loaded/refreshed.</para>
    /// </summary>
    protected ValueBag<Action<object[]>> _listeners = new();

    /// <summary>
    ///   <para>The asset manager's cache reader-writer lock.</para>
    /// </summary>
    protected readonly ReaderWriterLockSlim _cacheRwl = new();

    /// <summary>
    ///   <para>Determines whether this asset manager has been disposed of.</para>
    /// </summary>
    protected bool disposed;

    public bool Reloadable { get; protected set; } = true;

    protected ValueBag<string>? ToReload;

    public MultiTaskManager<object?>? LoadTasksManager;

    public bool Loading => LoadTasksManager?.InProgress ?? false;

    /// <summary>
    ///   <para>Gets the asset manager's display name.</para>
    /// </summary>
    public abstract string DisplayName { get; }

    /// <summary>
    ///   <para>Returns the string representation of this asset manager: its display name along with the registered prefix.</para>
    /// </summary>
    /// <returns>The string representation of this asset manager.</returns>
    public override string ToString()
        => RegisteredPrefix is null ? $"{DisplayName} (no prefix)" : $"{DisplayName} ({RegisteredPrefix}:/)";

    /// <summary>
    ///   <para>Releases all resources used by the asset manager.</para>
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, true)) return;
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///   <para>Releases the unmanaged resources used by the asset manager and optionally releases the managed resources.</para>
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;
        Assets.UnRegisterAssetManager(this);
        _cache = null!;
    }

    public async Task<object?> LoadIntoCacheAsync(string path)
    {
        if (path.Contains('\\')) path = path.Replace('\\', '/');

        try
        {
            object asset = await LoadNewAssetAsync(path);
            if (_cache.TryGetValue(path, out object? previousAsset))
            {
                _cacheRwl.EnterWriteLock();
                _cache[path] = asset;
                return previousAsset;
            }
            else
            {
                _cacheRwl.EnterWriteLock();
                _cache[path] = asset;
                return null;
            }

        }
        finally
        {
            _cacheRwl.ExitWriteLock();
        }
    }

    protected void AddToToReload()
    {
        ToReload ??= new();
    }

    public void ReloadAssets()
    {
        if (!Reloadable) return;
        ReloadAssets(LoadAssetsCore());
    }

    public void ReloadAssets(string[] assetsPaths)
    {
        if (!Reloadable) return;
        ToReload = new(assetsPaths);
        ApplyAssetsReload();
    }

    public void ApplyAssetsReload()
    {
        if (ToReload is null) return;
        LoadTasksManager ??= new();
        foreach (string assetPath in ToReload.Span) LoadTasksManager.Add(LoadIntoCacheAsync(assetPath));
        ToReload = null;
    }

    protected abstract string[] LoadAssetsCore();

    public void Update()
    {
        if (LoadTasksManager is null) return;
        LoadTasksManager.Update();
        if (!LoadTasksManager.InProgress)
        {
            object[] previousAssets = LoadTasksManager.Results.OfType<object>().ToArray();
            foreach (Action<object[]> listener in _listeners.Span)
                listener(previousAssets);

            LoadTasksManager = null;
        }
    }


    /// <summary>
    ///   <para>Returns a new asset, loaded from the specified path, or throws an exception if the specified asset is not found.</para>
    /// </summary>
    /// <param name="assetPath">A relative path to the asset to load.</param>
    /// <returns>A new asset, loaded from the specified path.</returns>
    protected abstract ValueTask<object> LoadNewAssetAsync(string assetPath);


    public T Load<T>(string assetPath)
    {
        object asset = _cache[assetPath];
        if (asset is T tAsset) return tAsset;
        throw new AssetTypeMismatchException(this, assetPath, typeof(T), asset.GetType());
    }

    public void AddListener(Action<object[]> listener) => _listeners.Add(listener);
}