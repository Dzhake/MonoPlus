using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Linguini.Bundle;
using Linguini.Bundle.Builder;
using MonoPlus.ModSystem;
using Serilog;

namespace MonoPlus.LocalizationSystem;

/// <summary>
/// Class to manage localization.
/// </summary>
public static class Locale
{
    /// <summary>
    /// Bundle which contains all currently loaded strings, but fallback ones.
    /// </summary>
    public static FluentBundle? MainBundle;

    /// <summary>
    /// Fallback bundle, in case some string was not found.
    /// </summary>
    public static FluentBundle? FallbackBundle;

    /// <summary>
    /// Whether to use <see cref="FallbackBundle"/> (load to and read from it).
    /// </summary>
    public static bool UseFallbackBundle => CurrentLanguage != FallbackLanguage && LoadFallback;

    /// <summary>
    /// Whether to load fallback strings. If <see langword="false"/>, the game only loads required strings. If <see langword="true"/>, the game will also load whole english bundle, and fallback to it.
    /// </summary>
    public static readonly bool LoadFallback = true;

    /// <summary>
    /// Currently selected language identifier, which should match filenames of loaded strings (without extension and period).
    /// </summary>
    public static string CurrentLanguage = "en";

    /// <summary>
    /// Fallback language identifier, which should match filenames of strings loaded to fallback bundle (without extension and period).
    /// </summary>
    public static readonly string FallbackLanguage = "en";

    /// <summary>
    /// <see cref="HashSet{T}"/> of strings, which were used as keys during this program launch, requesting a message, but message with the specified key was not found.
    /// </summary>
    public static readonly HashSet<string> MissingStrings = new();

    /// <summary>
    /// Creates new empty <see cref="MainBundle"/> (and <see cref="FallbackBundle"/> if <see cref="UseFallbackBundle"/> is <see langword="true"/>).
    /// </summary>
    public static void CreateBundle()
    {
        MainBundle = LinguiniBuilder.Builder().CultureInfo(CultureInfo.InvariantCulture).SkipResources().UncheckedBuild();
        if (UseFallbackBundle) FallbackBundle = LinguiniBuilder.Builder().CultureInfo(CultureInfo.InvariantCulture).SkipResources().UncheckedBuild();
    }

    /// <summary>
    /// Recreates the bundle, and loads localization files from vanilla and all mods.
    /// </summary>
    /// <param name="resetMissingStrings">Whether to also clear <see cref="MissingStrings"/>, duplicating the warnings about missing strings.</param>
    public static void FullLoad(bool resetMissingStrings = false)
    {
        if (resetMissingStrings) MissingStrings.Clear();
        CreateBundle();
        
        //Load localization for vanilla.
        LoadDir(GetLocalizationDir(AppContext.BaseDirectory));
        
        //Load localization from mods.
        if (ModManager.Mods is not null)
            foreach (Mod mod in ModManager.Mods.Values)
                LoadDir(GetLocalizationDir(mod.Directory));
    }

    /// <summary>
    /// Reloads localization only for the specified <paramref name="mod"/>. Doesn't not remove old keys which don't exist now.
    /// </summary>
    /// <param name="mod">Mod which localization files to reload.</param>
    public static void SoftReloadMod(Mod mod)
    {
        LoadDir(GetLocalizationDir(mod.Directory));
    }

    /// <summary>
    /// Get the specified <paramref name="dir"/> joined with "Localization".
    /// </summary>
    /// <param name="dir"><see cref="Directory"/> path to join.</param>
    /// <returns><see cref="Directory"/> path of the specified <paramref name="dir"/> joined with "Localization".</returns>
    public static string GetLocalizationDir(string dir) => Path.Join(dir, "Localization");

    /// <summary>
    /// Load localization files from the specified <paramref name="dir"/>. Recursive. Does not load files which don't match <see cref="ShouldLoad"/>.
    /// </summary>
    /// <param name="dir"><see cref="Directory"/> path with files which should be loaded as localization files.</param>
    public static void LoadDir(string dir)
    {
        if (!Directory.Exists(dir)) return;
        IEnumerable<string> files = Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories);
        foreach (string file in files) LoadFile(file);
    }

    /// <summary>
    /// Loads the specified <paramref name="file"/> as localization file. Does not load files which don't match <see cref="ShouldLoad"/>.
    /// </summary>
    /// <param name="file"><see cref="File"/> path to load from.</param>
    public static void LoadFile(string file)
    {
        if (!File.Exists(file)) throw new FileNotFoundException($"File at the specified path does not exist: {file}", file);
        string fileNameNoExt = Path.GetFileNameWithoutExtension(file);
        if (!ShouldLoad(fileNameNoExt, out bool fallback)) return;
        FileStream stream = new(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        Load(new StreamReader(stream), fallback);
    }

    /// <summary>
    /// Checks if file should be loaded as localization file, and returns whether to load it to <see cref="FallbackBundle"/> or <see cref="MainBundle"/>.
    /// </summary>
    /// <param name="fileNameNoExt">Short file name without extension and a period. Example: "en".</param>
    /// <param name="fallback">Whether to load file to <see cref="FallbackBundle"/> or <see cref="MainBundle"/>.</param>
    /// <returns>Whether file should be loaded as localization file.</returns>
    public static bool ShouldLoad(string fileNameNoExt, out bool fallback)
    {
        fallback = false;
        if (fileNameNoExt == CurrentLanguage) return true;
        if (fileNameNoExt == FallbackLanguage)
        {
            fallback = true;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Loads resources from specified <paramref name="stream"/> into <see cref="MainBundle"/>.
    /// </summary>
    /// <param name="stream">Stream, reading .ftl asset.</param>
    /// <param name="fallback">Whether load to fallback bundle. Does nothing if <see cref="UseFallbackBundle"/> is false.</param>
    /// <exception cref="InvalidOperationException"><see cref="MainBundle"/> is null.</exception>
    public static void Load(TextReader stream, bool fallback)
    {
        if (fallback)
        {
            if (!UseFallbackBundle) return;
            if (FallbackBundle is null) throw new InvalidOperationException("FallbackBundle is null, but Load was called with 'fallback'");
            FallbackBundle.AddResourceOverriding(stream);
        }
        else
        {
            if (MainBundle is null) throw new InvalidOperationException("Bundle is null");
            MainBundle.AddResourceOverriding(stream);
        }
    }

    /// <summary>
    /// Get string with specified key.
    /// </summary>
    /// <param name="key">Id of string to get.</param>
    /// <returns>String from the <see cref="MainBundle"/>, matching the <paramref name="key"/>, or "{key}" if not found.</returns>
    /// <exception cref="InvalidOperationException"><see cref="MainBundle"/> is null.</exception>
    public static string Get(string key)
    {
        if (MainBundle is not null)
        {
            MainBundle.TryGetMessage(key, null, out _, out string? message);
            if (message is not null) return message;            
        }

        if (UseFallbackBundle && FallbackBundle is not null)
        {
            FallbackBundle.TryGetMessage(key, null, out _, out string? message);
            if (message is not null) return message;
        }

        if (MissingStrings.Contains(key)) return $"{{{key}}}";
        
        Log.Warning("Message with key {Key} not found", key);
        MissingStrings.Add(key);
        return $"{{{key}}}";
    }
}
