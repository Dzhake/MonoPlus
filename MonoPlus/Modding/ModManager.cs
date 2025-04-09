using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MonoPlus.Modding;

public static class ModManager
{
    public static List<Mod> CurrentMods = new();
    public static HashSet<string> EnabledModsNames = new();
    public static string ModsDirectory = $"{AppContext.BaseDirectory}Mods/";

    public static void Initialize()
    {
        foreach (ReadOnlySpan<char> modDir in Directory.EnumerateDirectories(ModsDirectory, "*", SearchOption.TopDirectoryOnly))
            LoadMod(modDir);
    }

    private static void LoadMod(ReadOnlySpan<char> modDir)
    {
        Mod mod = new();
        string configPath = GetModConfigPath(modDir);
        if (!File.Exists(configPath)) throw new InvalidModConfigurationException(configPath, "File not found");

        mod.Config = JsonSerializer.Deserialize<ModConfig>(File.ReadAllText(configPath)) ?? throw new InvalidModConfigurationException(configPath, "Deserializer returned null.");
        var config = mod.Config;
        foreach (ReadOnlySpan<char> relativeDllPath in config.DllFiles)
        {
            string absoluteDllPath = Path.Join(modDir, relativeDllPath);
            if (!File.Exists(absoluteDllPath)) throw new InvalidModConfigurationException(configPath, $"Config contains a reference to .dll file which was not found: {absoluteDllPath}");
        }
    }

    public static string GetModConfigPath(ReadOnlySpan<char> modDir) => $"{modDir}config.json";

    public static void PreLoadContent()
    {

    }
}
