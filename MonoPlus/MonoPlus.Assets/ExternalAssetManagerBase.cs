using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using MonoPlus.Graphics;

namespace MonoPlus.Assets;

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
            return await new StreamReader(info.AssetStream).ReadToEndAsync();
        case AssetType.BINARY:
            return info.AssetStream.ToByteArrayDangerous();
        default:
            throw new UnknownAssetFormatException(this, assetPath);
        }
    }

    public struct ExternalAssetInfo
    {
        public Stream? AssetStream;
        public AssetFormat Format;

        public ExternalAssetInfo(Stream? assetStream, AssetFormat format)
        {
            AssetStream = assetStream;
            Format = format;
        }

    }
}