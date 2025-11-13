using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
 using System.IO;

namespace MonoPlus.AssetsSystem;

/// <summary>
/// Util methods related to <see cref="Assets"/> and <see cref="AssetManager"/>s.
/// </summary>
public static class AssetsUtils
{
    /// <summary>
    /// Detects asset format based on the specified <paramref name="filePath"/>.
    /// </summary>
    /// <param name="filePath"><see cref="File"/> path, or file name (might be only extension, but must include period).</param>
    /// <returns>Detected <see cref="AssetType"/>.</returns>
    [Pure] public static AssetType DetectTypeByPath(ReadOnlySpan<char> filePath) => DetectTypeByExtension(Path.GetExtension(filePath));

    /// <summary>
    /// Detects asset format based on the specified <paramref name="extension"/>.
    /// </summary>
    /// <param name="extension">File extension with period (e.g.: ".png").</param>
    /// <returns>Detected <see cref="AssetType"/>. Returns <see cref="AssetType.Unknown"/> for custom formats.</returns>
    [Pure]
    public static AssetType DetectTypeByExtension(ReadOnlySpan<char> extension)
    {
        extension = extension[1..]; //ok yes this is a crime but it saves a dot per each 
        return extension switch
        {
            "mp3" => AssetType.Audio,
            "ogg" => AssetType.Audio,
            "wav" => AssetType.Audio,

            "png" => AssetType.Image,
            "jpg" or "jpeg" => AssetType.Image,

            "txt" => AssetType.Text,
            "csv" => AssetType.Text,
            "json" => AssetType.Text,
            "yaml" => AssetType.Text,
            "xml" => AssetType.Text,

            "bin" or "bytes" => AssetType.Binary,

            "mgfx" => AssetType.Effect,

            _ => AssetType.Unknown,
        };
    }
}