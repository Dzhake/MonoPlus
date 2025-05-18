using Linguini.Bundle;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace MonoPlus.AssetsManagement;

/// <summary>
///   <para>Defines all the supported asset data formats.</para>
/// </summary>
public enum AssetFormat
{
    /// <summary>
    /// Unknown asset format, not supported by <see cref="MonoPlus.AssetsManagement"/>
    /// </summary>
    Unknown = 0,

    // [1, 32] - Audio

    /// <summary>
    /// Audio .mp3
    /// </summary>
    Mp3 = 1,

    /// <summary>
    /// Audio .ogg/vorbis
    /// </summary>
    Ogg = 2,

    /// <summary>
    /// Audio .wav
    /// </summary>
    Wav = 3,

    // [33, 64] - Image
    /// <summary>
    /// Image/Texture2D .png
    /// </summary>
    Png = 33,

    /// <summary>
    /// Image/Texture2D .jpg
    /// </summary>
    Jpeg = 34,

    // [65, 96] - video formats

    /// <summary>
    /// Video .mp4
    /// </summary>
    Mp4 = 65,

    // [97, 128] - text formats

    /// <summary>
    /// Text .txt
    /// </summary>
    Txt = 97,

    /// <summary>
    /// Text .csv
    /// </summary>
    Csv = 98,

    /// <summary>
    /// Text .json
    /// </summary>
    Json = 99,

    /// <summary>
    /// Text .yaml
    /// </summary>
    Yaml = 100,

    /// <summary>
    /// Text .xml
    /// </summary>
    Xml = 101,

    // [129, 160] - binary format

    /// <summary>
    /// Binary .bin
    /// </summary>
    Bin = 129,

    // [161, 192] - shader format (i really don't need that much reserved formats. Whatever)

    /// <summary>
    /// Shader .mgfx
    /// </summary>
    Mgfx = 161,

    // [193, 224] - fluent format

    /// <summary>
    /// <see cref="Localization.Locale"/>/Fluent .ftl
    /// </summary>
    Ftl = 193,
}

/// <summary>
///   <para>Defines the types of assets.</para>
/// </summary>
public enum AssetType
{
    /// <summary>
    /// Unknown asset type, not supported by <see cref="MonoPlus.AssetsManagement"/>.
    /// </summary>
    UNKNOWN,

    /// <summary>
    /// Audio asset type, converted into <see cref="SoundEffect"/>.
    /// </summary>
    AUDIO,

    /// <summary>
    /// Image asset type, conveted into <see cref="Texture2D"/>.
    /// </summary>
    IMAGE,

    /// <summary>
    /// Video asset type, TODO NOT YET SUPPORTED BECAUSE VIDEOS ARE NOT SUPPORTED BY MONOGAME.
    /// </summary>
    VIDEO,

    /// <summary>
    /// Text asset type, converted into <see cref="string"/>.
    /// </summary>
    TEXT,

    /// <summary>
    /// Binary asset type, converted into <see cref="T:byte[]"/>.
    /// </summary>
    BINARY,

    /// <summary>
    /// Effect asset type, converted into <see cref="Effect"/>.
    /// </summary>
    EFFECT,

    /// <summary>
    /// Fluent file asset type, loaded directly into <see cref="FluentBundle"/> if filename matches currently selected language
    /// </summary>
    LOCALIZATION,
}
