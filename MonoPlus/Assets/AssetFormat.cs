namespace MonoPlus.AssetsManagment;

/// <summary>
///   <para>Defines all the supported asset data formats.</para>
/// </summary>
public enum AssetFormat
{
    Unknown = 0,

    // [1, 32] - reserved for direct mapping to and from AssetFormat
    Mp3 = 1,
    Ogg = 2,
    Wav = 3,

    // [33, 64] - reserved for direct mapping to and from AssetFormat
    Png = 33,
    Jpeg = 34,

    // [65, 96] - video formats
    Mp4 = 65,

    // [97, 128] - text formats
    Txt = 97,
    Csv = 98,
    Json = 99,
    Yaml = 100,
    Xml = 101,

    // [129, 160] - binary format
    Bin = 129,

    // [161, 192] - shader format (i really don't need that much reserved formats. Whatever)
    Mgfx = 161,

    // [193, 224]
    Ftl = 193,
}
/// <summary>
///   <para>Defines the types of assets.</para>
/// </summary>
public enum AssetType
{
    UNKNOWN,
    AUDIO,
    IMAGE,
    VIDEO,
    TEXT,
    BINARY,
    EFFECT,
    LOCALIZATION,
}
