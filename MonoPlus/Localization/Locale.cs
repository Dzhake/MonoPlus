using System;
using System.Globalization;
using System.IO;
using Linguini.Bundle;
using Linguini.Bundle.Builder;
using Linguini.Bundle.Errors;
using Serilog;

namespace MonoPlus.Localization;

/// <summary>
/// Class to manage localization.
/// </summary>
public static class Locale
{
    /// <summary>
    /// Bundle which contains all currently loaded strings.
    /// </summary>
    private static FluentBundle? Bundle;

    /// <summary>
    /// Currently selected language identifier, which should match filenames of loaded string.
    /// </summary>
    public static string CurrentLanguage = "en";

    /// <summary>
    /// Initializes <see cref="Bundle"/>.
    /// </summary>
    public static void Initialize()
    {
        Bundle = LinguiniBuilder.Builder().CultureInfo(CultureInfo.InvariantCulture).SkipResources().UncheckedBuild();
    }

    /// <summary>
    /// Loads resources from specified <paramref name="stream"/> into <see cref="Bundle"/>.
    /// </summary>
    /// <param name="stream">Stream, reading .ftl asset.</param>
    /// <exception cref="InvalidOperationException"><see cref="Bundle"/> is null.</exception>
    public static void Load(StreamReader stream)
    {
        if (Bundle is null) throw new InvalidOperationException("Bundle is null, but Load was called");
        Bundle.AddResource(stream, out var errors);

        if (errors is null) return;
        foreach (FluentError error in errors)
            Log.Error("{Error}", error.ToString());
    }

    /// <summary>
    /// Get string with specified key.
    /// </summary>
    /// <param name="key">Id of string to get.</param>
    /// <returns>String from the <see cref="Bundle"/>, matching the <paramref name="key"/>, or "{key}" if not found.</returns>
    /// <exception cref="InvalidOperationException"><see cref="Bundle"/> is null.</exception>
    public static string Get(string key)
    {
        if (Bundle is null) throw new InvalidOperationException("Bundle is null, but Get was called");

        string? message = Bundle.GetMessage(key);
        if (message is not null) return message;

        Log.Warning("Message with key {Key} not found", key);
        return $"{{{key}}}";
    }
}
