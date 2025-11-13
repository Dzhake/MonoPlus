using System.Text.Json;

namespace MonoPlus.Utils.General;

/// <summary>
/// Small class for caching and reusing <see cref="JsonSerializerOptions"/>.
/// </summary>
public static class Json
{
    /// <summary>
    /// Common <see cref="JsonSerializerOptions"/> for use when you [de]serialize with <see cref="JsonSerializer"/>.
    /// </summary>
    public static readonly JsonSerializerOptions Common = new() { AllowTrailingCommas = true};

    /// <summary>
    /// <see cref="Common"/> for cases when you want to write clean and readable JSON.
    /// </summary>
    public static readonly JsonSerializerOptions Readable = new(Common) { WriteIndented = true, RespectNullableAnnotations = true};

    /// <summary>
    /// <see cref="Readable"/>, but includes serializing and deserializing fields.
    /// </summary>
    public static readonly JsonSerializerOptions WithFields = new(Readable) { IncludeFields = true };
}
