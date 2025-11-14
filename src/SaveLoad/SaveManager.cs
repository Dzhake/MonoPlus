using System;
using System.IO;
using System.Text.Json;
using Monod.Utils.General;

namespace Monod.SaveSystem;

/// <summary>
/// Class for saving and loading data using <see cref="JsonSerializer"/>.
/// </summary>
public static class SaveManager
{
    /// <summary>
    /// <see cref="Directory"/> path where all saves should be located. Directory called "Saves" next to <see cref="AppContext.BaseDirectory"/> by default.
    /// </summary>
    public static string SavesLocation = Environment.GetEnvironmentVariable("SavesDir") ?? Path.Join(AppContext.BaseDirectory, "Saves");
    
    /// <summary>
    /// Deserializes <see cref="File"/> at the specified <paramref name="saveLocation"/> as <typeparamref name="T"/> , and returns the deserialized value, or <see langword="null"/> if file not found.
    /// </summary>
    /// <param name="saveLocation"><see cref="File"/> path to the file with serialized <typeparamref name="T"/>.</param>
    /// <typeparam name="T">Type of the serialized object.</typeparam>
    /// <returns>Deserialized object.</returns>
    public static T? Load<T>(string saveLocation)
    {
        if (!File.Exists(saveLocation)) return default(T);
        string data = File.ReadAllText(saveLocation);
        return JsonSerializer.Deserialize<T>(data, Json.Common);
    }

    /// <summary>
    /// Serializes <paramref name="data"/> to the <see cref="File"/> at <paramref name="saveLocation"/>.
    /// </summary>
    /// <param name="saveLocation"><see cref="File"/> path where to save the <paramref name="data"/>.</param>
    /// <param name="data">Json-serializable object to save.</param>
    /// <typeparam name="T">Type of the <paramref name="data"/>.</typeparam>
    public static void Save<T>(string saveLocation, T data)
    {
        File.WriteAllText(saveLocation, JsonSerializer.Serialize(data, Json.Common));
    }
}