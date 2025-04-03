﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace MonoPlus.Assets;

/// <summary>
///   <para>Represents an asset manager, that loads assets from a directory in the file system.</para>
/// </summary>
public sealed class FileSystemAssetManager : ExternalAssetManagerBase
{
    /// <summary>
    ///   <para>Gets the full path to the directory that this asset manager loads assets from.</para>
    /// </summary>
    public string DirectoryPath { get; }

    /// <inheritdoc/>
    public override string DisplayName => $"\"{DirectoryPath}{Path.DirectorySeparatorChar}**\"";

    private readonly object stateLock = new();
    private FileSystemWatcher? _watcher;

    /// <summary>
    ///   <para>Gets or sets whether the changes in files of the directory should trigger a reload.</para>
    /// </summary>
    public bool ObserveChanges
    {
        get => _watcher is not null;
        set
        {
            ObjectDisposedException.ThrowIf(disposed != 0, this);

            lock (stateLock)
            {
                if (_watcher is not null == value) return;
                if (value) InitWatcher();
                else DisposeWatcher();
            }
        }
    }

    /// <summary>
    ///   <para>Initializes a new instance of the <see cref="FileSystemAssetManager"/> class with the specified <paramref name="directoryPath"/>.</para>
    /// </summary>
    /// <param name="directoryPath">A path to the directory to load assets from.</param>
    /// <exception cref="ArgumentNullException"><paramref name="directoryPath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="directoryPath"/> is not a valid directory path.</exception>
    /// <exception cref="NotSupportedException"><paramref name="directoryPath"/> contains a colon (":") that is not part of a volume identifier (for example, "c:\").</exception>
    /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    public FileSystemAssetManager(string directoryPath)
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
        if (_watcher != sender) return;
        RefreshAssetAtFilePath(args.FullPath);
    }
    private void OnFileRenamed(object? sender, RenamedEventArgs args)
    {
        if (_watcher != sender) return;
        RefreshAssetAtFilePath(args.OldFullPath);
        RefreshAssetAtFilePath(args.FullPath);
    }
    private void RefreshAssetAtFilePath(string filePath)
    {
        string assetPath = Path.GetRelativePath(DirectoryPath, filePath).Replace('\\', '/');
        RefreshAsset(assetPath);
    }

    /// <inheritdoc/>
    [Pure] protected override ValueTask<ExternalAssetInfo> GetAssetInfo(string assetPath)
    {
        if (!Directory.Exists(DirectoryPath)) return default;

        string[] matchedFiles = Directory.GetFiles(DirectoryPath, assetPath + ".*");
        switch (matchedFiles.Length)
        {
            case >= 2:
                throw new DuplicateAssetException(this, DirectoryPath+assetPath, matchedFiles.Select(Path.GetExtension).ToArray()!);
            case 0:
                throw new AssetNotFoundException(this, DirectoryPath + assetPath);
        }

        string mainPath = matchedFiles[0];
        return new(new ExternalAssetInfo(File.OpenRead(mainPath), AssetFormatUtils.DetectFormatByPath(mainPath)));
    }

    public override void PreloadAssets()
    {
        if (!Directory.Exists(DirectoryPath)) return;
        int rootPathLength = DirectoryPath.Length;

        foreach (string file in Directory.GetFiles(DirectoryPath, "*", SearchOption.AllDirectories))
            LoadIntoCache(Path.ChangeExtension(file.Remove(0, rootPathLength), null)); //Remove everything but relative directory, remove extension
            
    }
}