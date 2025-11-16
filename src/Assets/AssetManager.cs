using System;
using System.Collections.Generic;
using System.Threading;
using Monod.AssetsSystem;
using Monod.Utils.Collections;
using Serilog;

namespace MonoPlus.AssetsSystem;

/// <summary>
/// Class for loading, storing, caching, and accessing various assets.
/// </summary>
public class AssetManager : IDisposable
{
    /// <summary>
    /// <see cref="IAssetFilter"/> used for this <see cref="AssetManager"/>.
    /// </summary>
    public IAssetFilter? Filter;

    /// <summary>
    /// <see cref="IAssetLoader"/> used for this <see cref="AssetManager"/>.
    /// </summary>
    public IAssetLoader Loader;

    /// <summary>
    /// List of objects which listen to assets being reloaded, and load the assets again on reload.
    /// </summary>
    private readonly IndexedList<IAssetListener> listeners = new();

    /// <summary>
    /// Whether this asset manager has been disposed.
    /// </summary>
    protected bool disposed;

    /// <summary>
    /// Unique prefix for this <see cref="AssetManager"/>, which should be used for <see cref="Assets.Get{T}"/>
    /// </summary>
    public string? Prefix = null;

    /// <summary>
    ///   Get the asset manager's display name.
    /// </summary>
    public string DisplayName => Loader.DisplayName;
    
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
    /// <param name="path">Path of the asset in this <see cref="AssetManager"/></param>
    /// <returns>Asset at the specified <paramref name="path"/></returns>
    /// <exception cref="AssetTypeMismatchException"><typeparamref name="T"/> does not match type of the loaded asset.</exception>
    public T Get<T>(string path)
    {
        object? asset = Loader.GetAsset(path);
        return asset switch
        {
            null => Assets.NotFoundPolicy switch
            {
                Assets.NotFoundPolicyType.Exception => throw new AssetNotFoundException(this, path),
                Assets.NotFoundPolicyType.Fallback => GetDefault<T>() ?? throw new AssetFallbackNotFoundException(this, typeof(T)),
                _ => throw new IndexOutOfRangeException($"{nameof(Assets)}.{nameof(Assets.NotFoundPolicy)} was not any known type: {Assets.NotFoundPolicy}")
            },
            T castedAsset => castedAsset,
            _ => throw new AssetTypeMismatchException(this, path, typeof(T), asset.GetType())
        };
    }

    /// <summary>
    /// Same as <see cref="Get{T}"/>, but returns <see langword="null"/> if asset was not found, instead of using <see cref="Assets.NotFoundPolicy"/>.
    /// </summary>
    /// <typeparam name="T"><see cref="Type"/> of the asset to return. Throws an exception if types of asset in memory and requested types don't match.</typeparam>
    /// <param name="path">Path of the asset in this <see cref="AssetManager"/>.</param>
    /// <returns>Asset at the specified <paramref name="path"/>, or <see langword="null"/> if not found.</returns>
    /// <exception cref="AssetTypeMismatchException"><typeparamref name="T"/> does not match type of the loaded asset.</exception>
    public T? GetOrDefault<T>(string path)
    {
        object? asset = Loader.GetAsset(path);
        return asset switch
        {
            null => default,
            T castedAsset => castedAsset,
            _ => throw new AssetTypeMismatchException(this, path, typeof(T), asset.GetType())
        };
    }
    
    /// <summary>
    /// Returns default (fallback) asset with the specified <typeparamref name="T"/> for asset that you could not be found.
    /// </summary>
    /// <typeparam name="T">Type of the fallback.</typeparam>
    /// <returns>Fallback asset for that type.</returns>
    /// <exception cref="AssetFallbackNotFoundException">Thrown if asset fallback was not found, or could not be loaded.</exception>
    public static T? GetDefault<T>()
    {
        return Assets.GetOrDefault<T>($"fallbacks:/{typeof(T)}");
    }

    /// <summary>
    /// Reload all assets loaded by this <see cref="AssetManager"/>.
    /// </summary>
    public void ReloadAllAssets()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (!MonodMain.HotReload) return;
        
        Loader.LoadAssetManifests();
        LoadAssets();
        InvokeListeners();
    }
    
    
    /// <summary>
    /// Loads all assets related to this manager, e.g. all files from its directory for file-based all manager.
    /// </summary>
    public void LoadAssets()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        Loader.LoadAssets();
        Log.Information("{AssetManager} is loading assets", this);
        /*LoadedAmount = 0;
        string[] assetPaths = GetAllAssetsPaths();
        if (Filter != null) assetPaths = assetPaths.Where(Filter.ShouldLoad).ToArray();
        TotalAmount = assetPaths.Length;
        foreach (string assetPath in assetPaths)
            MainThread.Add(Task.Run(async () => await LoadIntoCacheAsync(Path.GetFileNameWithoutExtension(assetPath))));*/
    }

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
    /// Invoke all listeners, to make them reload assets.
    /// </summary>
    private void InvokeListeners()
    {
        foreach (IAssetListener assetListener in listeners)
            assetListener.LoadAssets(true);
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
    }

    /// <summary>
    /// Deconstructor for <see cref="AssetManager"/>, which doesn't dispose managed resources.
    /// </summary>
    ~AssetManager()
    {
        Dispose(false);
    }
}