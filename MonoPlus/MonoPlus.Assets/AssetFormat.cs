namespace MonoPlus.Assets;

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
}
/// <summary>
///   <para>Defines the types of assets.</para>
/// </summary>
public enum AssetType
{
    UNKNOWN = 0,
    AUDIO = 1,
    IMAGE = 2,
    VIDEO = 3,
    TEXT = 4,
    BINARY = 5,
}
