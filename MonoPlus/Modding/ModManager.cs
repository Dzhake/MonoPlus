using System;
using System.Collections.Generic;
using System.IO;

namespace MonoPlus.Modding;

public static class ModManager
{
    public static List<Mod> CurrentMods = new();
    public static HashSet<string> EnabledModsNames = new();
    public static string ModsDirectory = $"{AppContext.BaseDirectory}Mods/";

    public static void Initialize()
    {
        foreach (string modDir in Directory.EnumerateDirectories(ModsDirectory, "*", SearchOption.TopDirectoryOnly))
            LoadMod(modDir);
    }

    private static void LoadMod(string dir)
    {
        Mod mod = new();
        string configPath = GetModConfigPath(dir);
        if (File.Exists(configPath))
        {
            mod.Config = new(File.OpenRead(configPath), configPath); //Stream is closed in ModConfig's ctor
        }
    }

    public static string GetModConfigPath(string modDir) => $"{modDir}config.json";

    public static void PreLoadContent()
    {

    }
}
