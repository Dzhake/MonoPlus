﻿using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using MonoPlus.Graphics;
using MonoPlus.Localization;

namespace MonoPlus.AssetsManagement;

/// <summary>
///   <para>Implements a base for asset managers, that load assets from an external source.</para>
/// </summary>
public abstract class ExternalAssetManagerBase : AssetManager
{
    /// <summary>
    ///   <para>Gets information about an external asset at the specified path.</para>
    /// </summary>
    /// <param name="assetPath">A relative path to the external asset to load.</param>
    /// <returns>An <see cref="ExternalAssetInfo"/> structure representing information about the specified external asset.</returns>
    [Pure] protected abstract ValueTask<ExternalAssetInfo> GetAssetInfo(string assetPath);

    /// <inheritdoc/>
    protected override async ValueTask<object?> LoadNewAssetAsync(string assetPath)
    {
        ExternalAssetInfo info = await GetAssetInfo(assetPath);
        if (info.AssetStream is null) return null;

        switch (info.Format.ToType())
        {
        case AssetType.IMAGE:
            return Texture2D.FromStream(Renderer.device, info.AssetStream);
        case AssetType.AUDIO:
            return SoundEffect.FromStream(info.AssetStream);
        case AssetType.VIDEO:
            throw new NotImplementedException("Video loading is not implemented yet, and probably never will be.");
        case AssetType.TEXT:
            switch (Assets.ResourcePriority)
            {
                case Assets.ResourcePriorityType.Performance:
                    return await new StreamReader(info.AssetStream, Encoding.UTF8).ReadToEndAsync();
                case Assets.ResourcePriorityType.Memory:
                    return Encoding.UTF8.GetString(info.AssetStream.ToByteArrayDangerous());
            }
            break;
        case AssetType.BINARY:
            return info.AssetStream.ToByteArrayDangerous();
        case AssetType.EFFECT:
            return new Effect(Renderer.device, info.AssetStream.ToByteArrayDangerous());
        case AssetType.LOCALIZATION:
            if (Locale.CurrentLanguage == Path.GetFileNameWithoutExtension(assetPath))
                Locale.Load(new(info.AssetStream));
            return null;
        default:
            throw new UnknownAssetFormatException(this, assetPath);
        }

        throw new UnreachableException($"Asset type is {info.Format.ToType()}. I don't know how did this bypass \"default:\"");
    }

    /// <summary>
    /// Creates <see cref="FileSystemAssetManager"/> or <see cref="ZipArchiveAssetManager"/> for specified path.
    /// </summary>
    /// <param name="rootName">Folder name without trailing slash or zip archive name without extension (and dot).</param>
    /// <returns><see cref="FileSystemAssetManager"/> if "<paramref name="rootName"/>/" was found, <see cref="ZipArchiveAssetManager"/> if "<paramref name="rootName"/>.zip" was found and null if neither found.</returns>
    public static AssetManager? FolderOrZip(string rootName)
    {
        string folder = $"{rootName}/";
        string zip = $"{rootName}.zip";
        return Directory.Exists(folder) ? new FileSystemAssetManager(folder) :
            File.Exists(zip) ? new ZipArchiveAssetManager(zip) : null;
    }

    /// <summary>
    /// Represents information about external asset.
    /// </summary>
    public struct ExternalAssetInfo
    {
        /// <summary>
        /// Stream, reading the asset.
        /// </summary>
        public Stream? AssetStream;

        /// <summary>
        /// Format of the asset.
        /// </summary>
        public AssetFormat Format;

        /// <summary>
        /// Instances a new <see cref="ExternalAssetInfo"/> with specified <paramref name="assetStream"/> and <paramref name="format"/>.
        /// </summary>
        /// <param name="assetStream">Stream, reading the asset.</param>
        /// <param name="format">Format of the asset.</param>
        public ExternalAssetInfo(Stream? assetStream, AssetFormat format)
        {
            AssetStream = assetStream;
            Format = format;
        }
    }
}