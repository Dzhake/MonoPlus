﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace MonoPlus.AssetsManagment;

/// <summary>
///   <para>Represents an asset manager, that loads assets from a ZIP archive file in the file system.</para>
/// </summary>
public sealed class ZipArchiveAssetManager : ExternalAssetManagerBase
{
    /// <summary>
    ///   <para>Gets the full path to the archive file that this asset manager loads assets from.</para>
    /// </summary>
    public string ArchivePath { get; }

    /// <inheritdoc/>
    public override string DisplayName => $"\"{ArchivePath}\"";

    private readonly object stateLock = new();
    private FileSystemWatcher? _watcher;

    private Task? reloadTask;
    private int reloadVersion;
    private Dictionary<string, Entry>? _lookup;

    /// <summary>
    ///   <para>Gets or sets whether the changes in the archive file should trigger a reload.</para>
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
    ///   <para>Initializes a new instance of the <see cref="ZipArchiveAssetManager"/> class with the specified <paramref name="archivePath"/>.</para>
    /// </summary>
    /// <param name="archivePath"><see cref="File"/> path to the archive to load assets from.</param>
    /// <exception cref="ArgumentNullException"><paramref name="archivePath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="archivePath"/> is not a valid <see cref="File"/> path.</exception>
    /// <exception cref="NotSupportedException"><paramref name="archivePath"/> contains a colon (":") that is not part of a volume identifier (for example, "c:\").</exception>
    /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    public ZipArchiveAssetManager(string archivePath)
    {
        ArgumentNullException.ThrowIfNull(archivePath);
        ArchivePath = Path.GetFullPath(archivePath);
    }
    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) DisposeWatcher();
    }

    private void InitWatcher()
    {
        string directoryPath = Path.GetDirectoryName(ArchivePath)!;
        string fileName = Path.GetFileName(ArchivePath);
        FileSystemWatcher watcher = new (directoryPath, fileName);

        watcher.Created += OnFileChanged;
        watcher.Deleted += OnFileChanged;
        watcher.Changed += OnFileChanged;
        watcher.Renamed += OnFileChanged;

        watcher.EnableRaisingEvents = true;
        _watcher = watcher;
    }
    private void DisposeWatcher()
    {
        Interlocked.Exchange(ref _watcher, null)?.Dispose();
    }

    private void OnFileChanged(object? sender, FileSystemEventArgs args)
    {
        if (_watcher != sender) return;
        reloadTask = ReloadArchiveAsync();
    }

    private async Task ReloadArchiveAsync()
    {
        // Increment the version counter and use it as this operation's identifier
        int reloadId;
        Dictionary<string, Entry>? oldLookup;

        lock (stateLock)
        {
            reloadId = Interlocked.Increment(ref reloadVersion);
            oldLookup = _lookup;
        }

        // Read the entire archive, putting it in a byte[] for fast access
        byte[] archiveData = await File.ReadAllBytesAsync(ArchivePath);

        // If after reading the file this operation became outdated, stop
        if (Volatile.Read(ref reloadVersion) != reloadId) return;

        using MemoryStream memory = new(archiveData);
        using ZipArchive archive = new(memory);

        Dictionary<string, Entry> newLookup = [];

        foreach (ZipArchiveEntry zipEntry in archive.Entries)
        {
            // Open and decompress the entry, and store its info in entries
            Stream entryData = zipEntry.Open();
            uint crc32 = zipEntry.Crc32;

            ReadOnlySpan<char> zipEntryName = zipEntry.FullName.AsSpan();
            ReadOnlySpan<char> assetExtension = Path.GetExtension(zipEntryName);
            string assetName = zipEntryName[..^assetExtension.Length].ToString();

            // Get an asset entry, or default if not defined yet
            Entry entry = newLookup.GetValueOrDefault(assetName);

            // Set the associated fields
            entry.AssetData = entryData;
            entry.AssetCrc32 = crc32;
            entry.Format = AssetFormatUtils.DetectFormatByPath(assetExtension);

            //load entry into cache
            //LoadIntoCache(zipEntryName.ToString());
            //TODO: make a good way to preload assets instead of by path.

            // Overwrite entry if exists, or create if not found
            newLookup[assetName] = entry;

            // If at any point during decompression this operation became outdated, stop
            if (Volatile.Read(ref reloadVersion) != reloadId) return;
        }

        lock (stateLock)
        {
            if (Volatile.Read(ref reloadVersion) != reloadId) return;
            _lookup = newLookup;

            // If there was a different lookup before, see what's changed and trigger updates
            if (oldLookup is not null)
            {
                // Trigger removed or updated assets
                foreach (var (key, oldEntry) in oldLookup)
                    if (!newLookup.TryGetValue(key, out Entry newEntry) || !oldEntry.EqualsCrc32(newEntry))
                        RefreshAsset(key);

                // Trigger added assets
                foreach (var (key, _) in newLookup)
                    if (!oldLookup.ContainsKey(key))
                        RefreshAsset(key);
            }
        }
    }

    private ValueTask<Dictionary<string, Entry>> GetLookupAsync()
    {
        var entries = _lookup;
        if (entries is not null) return new(entries);

        lock (stateLock)
        {
            entries = _lookup;
            if (entries is not null) return new(entries);

            reloadTask ??= Task.Run(ReloadArchiveAsync);
            return GetLookupAsyncWait(reloadTask);
        }
    }
    private async ValueTask<Dictionary<string, Entry>> GetLookupAsyncWait(Task lookupReloadTask)
    {
        await lookupReloadTask;
        return await GetLookupAsync();
    }

    /// <inheritdoc/>
    [Pure] protected override async ValueTask<ExternalAssetInfo> GetAssetInfo(string assetPath)
    {
        var lookup = await GetLookupAsync();

        if (!lookup.TryGetValue(assetPath, out Entry entry) || entry.AssetData is null) return default;

        return new ExternalAssetInfo(entry.AssetData, entry.Format);
    }

    private struct Entry(Stream? assetData, uint assetCrc32, AssetFormat format)
    {
        public Stream? AssetData = assetData;
        public uint AssetCrc32 = assetCrc32;
        public AssetFormat Format = format;

        public readonly bool EqualsCrc32(in Entry other) => AssetCrc32 == other.AssetCrc32;
    }
}