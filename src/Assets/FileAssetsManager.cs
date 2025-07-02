using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using MonoPlus.GraphicsSystem;
using MonoPlus.LocalizationSystem;

namespace MonoPlus.AssetsSystem;

/// <summary>
///     <para>Represents an asset manager, that loads assets from a directory in the file system.</para>
/// </summary>
public class FileAssetsManager : AssetsManager
{
    /// <summary>
    ///   <para>Gets the full path to the directory that this asset manager loads assets from.</para>
    /// </summary>
    public string DirectoryPath { get; }

    /// <inheritdoc/>
    protected override string DisplayName => $"{DirectoryPath}{Path.DirectorySeparatorChar}**";

    private readonly Lock watcherLock = new();
    private FileSystemWatcher? _watcher;

    /// <summary>
    ///   <para>Gets or sets whether the changes in files of the directory should trigger a reload.</para>
    /// </summary>
    public bool ObserveChanges
    {
        get => _watcher is not null;
        set
        {
            ObjectDisposedException.ThrowIf(disposed, this);

            lock (watcherLock)
            {
                if (_watcher is not null == value) return;
                if (value) InitWatcher();
                else DisposeWatcher();
            }
        }
    }

    /// <summary>
    ///   <para>Initializes a new instance of the <see cref="FileAssetsManager"/> class with the specified <paramref name="directoryPath"/>.</para>
    /// </summary>
    /// <param name="directoryPath">A path to the directory to load assets from.</param>
    /// <exception cref="ArgumentNullException"><paramref name="directoryPath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="directoryPath"/> is not a valid directory path.</exception>
    /// <exception cref="NotSupportedException"><paramref name="directoryPath"/> contains a colon (":") that is not part of a volume identifier (for example, "c:\").</exception>
    /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    public FileAssetsManager(string directoryPath)
    {
        ArgumentNullException.ThrowIfNull(directoryPath);
        DirectoryPath = Path.GetFullPath(directoryPath);
    }
    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) DisposeWatcher();
    }

    /// <inheritdoc />
    protected override string[] GetAllAssetsPaths()
    {
        return Directory.EnumerateFiles(DirectoryPath, "*", SearchOption.AllDirectories).Select(file => Path.GetRelativePath(DirectoryPath, file)).ToArray();
    }

    private void InitWatcher()
    {
        _watcher = new(DirectoryPath);

        _watcher.Created += OnFileChanged;
        _watcher.Deleted += OnFileChanged;
        _watcher.Changed += OnFileChanged;
        _watcher.Renamed += OnFileRenamed;

        _watcher.EnableRaisingEvents = true;
    }

    private void DisposeWatcher()
    {
        Interlocked.Exchange(ref _watcher, null)?.Dispose();
    }

    private void OnFileChanged(object? sender, FileSystemEventArgs args)
    {
        if (!MonoPlusMain.HotReload || _watcher != sender) return;
        ReloadAsset(args.FullPath);
    }

    private void OnFileRenamed(object? sender, RenamedEventArgs args)
    {
        if (!MonoPlusMain.HotReload || _watcher != sender) return;
        ReloadAsset(args.OldFullPath);
        ReloadAsset(args.FullPath);
    }

    /// <inheritdoc />
    protected override async Task<object?> LoadNewAssetAsync(string assetPath)
    {
        ExternalAssetInfo? info = GetAssetInfo(assetPath);
        if (info is null) return null;
        
        switch (info.type)
        {
            case AssetType.Image:
                return Texture2D.FromStream(Renderer.device, info.stream);
            case AssetType.Audio:
                return SoundEffect.FromStream(info.stream);
            case AssetType.Text:
                return Assets.ResourcePriority switch
                {
                    Assets.ResourcePriorityType.Performance => Encoding.UTF8.GetString(info.stream.ToByteArrayDangerous()),
                    Assets.ResourcePriorityType.Memory => await new StreamReader(info.stream, Encoding.UTF8).ReadToEndAsync(),
                    _ => throw new IndexOutOfRangeException()
                };
            case AssetType.Binary:
                return info.stream.ToByteArrayDangerous();
            case AssetType.Effect:
                return new Effect(Renderer.device, info.stream.ToByteArrayDangerous());
            case AssetType.Unknown:
            default:
                if (Assets.CustomFormats.TryGetValue(Path.GetExtension(assetPath), out var fallbackFunc))
                    return fallbackFunc(info.stream);
                throw new UnknownAssetFormatException(this, assetPath);
        }
    }

    private ExternalAssetInfo? GetAssetInfo(string assetPath)
    {
        if (!Directory.Exists(DirectoryPath)) return null;

        string[] matchedFiles = Directory.GetFiles(DirectoryPath, assetPath + ".*");
        switch (matchedFiles.Length)
        {
            case >= 2:
                throw new DuplicateAssetException(this, DirectoryPath + assetPath,
                matchedFiles.Select(Path.GetExtension).ToArray()!);
            case 0:
                return null;
        }

        string filePath = matchedFiles[0];
        return new(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), AssetsUtils.DetectTypeByPath(filePath));
    }

    private record ExternalAssetInfo(FileStream stream, AssetType type)
    {
        /// <summary>
        /// <see cref="FileStream"/>, reading asset's data.
        /// </summary>
        public readonly FileStream stream = stream;
        
        /// <summary>
        /// Type of the asset.
        /// </summary>
        public readonly AssetType type = type;
    }
}