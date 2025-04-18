﻿﻿using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace MonoPlus.AssetsManagment;

/// <summary>
/// Util methods related to detecting <see cref="AssetFormat"/>
/// </summary>
public static class AssetFormatUtils
{
    // Order of bytes in an int32/int64 may differ depending on the machine's endianness

    /*private static readonly int OggHeader = ByteSequenceToInt32(+'O', +'g', +'g', +'S');
    private static readonly int RiffHeader = ByteSequenceToInt32(+'R', +'I', +'F', +'F');
    private static readonly int WaveHeader = ByteSequenceToInt32(+'W', +'A', +'V', +'E');
    private static readonly long PngHeader = ByteSequenceToInt64(0x89, +'P', +'N', +'G', +'\r', +'\n', 0x1A, +'\n');

    private static readonly int FirstThreeBytesMask = ByteSequenceToInt32(255, 255, 255, 0);

    // ID3 header used for metadata in MPEG files (fourth byte is irrelevant)
    private static readonly int Id3Header = ByteSequenceToInt32(+'I', +'D', +'3', 0);
    // 0xFF - marker, 0xD8 - Start of Image, 0xFF - marker (fourth byte is irrelevant)
    private static readonly int JpegHeader = ByteSequenceToInt32(0xFF, 0xD8, 0xFF, 0);

    [Pure] private static int ByteSequenceToInt32(byte a, byte b, byte c, byte d)
    {
        Span<byte> span = [a, b, c, d];
        return Unsafe.As<byte, int>(ref span[0]);
    }
    [Pure] private static long ByteSequenceToInt64(byte a, byte b, byte c, byte d, byte e, byte f, byte g, byte h)
    {
        Span<byte> span = [a, b, c, d, e, f, g, h];
        return Unsafe.As<byte, long>(ref span[0]);
    }

    /// <summary>
    ///   <para>The minimum amount of bytes required to detect a supported audio format.</para>
    /// </summary>
    public const int MinBytesToDetectAudioFormat = 12; // wav header length
    /// <summary>
    ///   <para>The minimum amount of bytes required to detect a supported image format.</para>
    /// </summary>
    public const int MinBytesToDetectImageFormat = 8; // png header length

    /// <summary>
    ///   <para>Attempts to identify the audio format of the specified <paramref name="rawData"/>.</para>
    /// </summary>
    /// <param name="rawData">A read-only span of bytes to identify the audio format of.</param>
    /// <returns>The identified audio format, or <see cref="AssetFormat.Unknown"/> if it could not be identified.</returns>
    [Pure] public static AssetFormat DetectAudioFormat(ReadOnlySpan<byte> rawData)
    {
        if (rawData.Length >= MinBytesToDetectAudioFormat)
        {
            ref int int32 = ref Unsafe.As<byte, int>(ref MemoryMarshal.GetReference(rawData));

            if (int32 == OggHeader)
                return AssetFormat.Ogg;

            if (int32 == RiffHeader && Unsafe.Add(ref int32, 2) == WaveHeader)
                return AssetFormat.Wav;

            if ((int32 & FirstThreeBytesMask) == Id3Header || rawData[0] == 0xFF && (rawData[1] | 1) == 0xFB)
                return AssetFormat.Mp3;
        }
        return AssetFormat.Unknown;
    }
    /// <summary>
    ///   <para>Attempts to identify the image format of the specified <paramref name="rawData"/>.</para>
    /// </summary>
    /// <param name="rawData">A read-only span of bytes to identify the image format of.</param>
    /// <returns>The identified image format, or <see cref="AssetFormat.Unknown"/> if it could not be identified.</returns>
    [Pure] public static AssetFormat DetectImageFormat(ReadOnlySpan<byte> rawData)
    {
        if (rawData.Length >= MinBytesToDetectImageFormat)
        {
            ref long int64 = ref Unsafe.As<byte, long>(ref MemoryMarshal.GetReference(rawData));

            if (int64 == PngHeader)
                return AssetFormat.Png;

            ref int int32 = ref Unsafe.As<long, int>(ref int64);

            if ((int32 & FirstThreeBytesMask) == JpegHeader)
                return AssetFormat.Jpeg;
        }
        return AssetFormat.Unknown;
    }*/

    /// <summary>
    /// Detects asset format based on <see cref="File"/>'s extension
    /// </summary>
    /// <param name="pathOrExtension"><see cref="File"/> path, or file name (might be only extension, but must include dot)</param>
    /// <returns></returns>
    [Pure] public static AssetFormat DetectFormatByPath(ReadOnlySpan<char> pathOrExtension)
    {
        pathOrExtension = Path.GetExtension(pathOrExtension);
        return pathOrExtension switch
        {
            ".mp3" => AssetFormat.Mp3,
            ".ogg" => AssetFormat.Ogg,
            ".wav" => AssetFormat.Wav,

            ".png" => AssetFormat.Png,
            ".jpg" or ".jpeg" => AssetFormat.Jpeg,

            ".mp4" => AssetFormat.Mp4,

            ".txt" => AssetFormat.Txt,
            ".csv" => AssetFormat.Csv,
            ".json" => AssetFormat.Json,
            ".yaml" => AssetFormat.Yaml,
            ".xml" => AssetFormat.Xml,

            ".bin" or ".bytes" => AssetFormat.Bin,

            ".mgfx" => AssetFormat.Mgfx, //Shader file generated by mgfxc

            ".ftl" => AssetFormat.Ftl,

            _ => AssetFormat.Unknown,
        };
    }

    /// <summary>
    ///   <para>Returns the asset type of the specified asset <paramref name="format"/>.</para>
    /// </summary>
    /// <param name="format">The asset format to get the type of.</param>
    /// <returns>The asset type of the specified asset <paramref name="format"/>.</returns>
    [Pure] public static AssetType ToType(this AssetFormat format)
        => (AssetType)((int)(format + 31) >> 5);

}