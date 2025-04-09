using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using MonoPlus.Assets;

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

    public static void LoadMod(ReadOnlySpan<char> modDir)
    {
        string configPath = GetModConfigPath(modDir);
        if (!File.Exists(configPath)) throw new InvalidModConfigurationException(configPath, "File not found");

        ModConfig config;

        using (var configStream = File.OpenRead(configPath))
        {
            config = JsonSerializer.Deserialize<ModConfig>(configStream) ?? throw new InvalidModConfigurationException(configPath, "Deserializer returned null.");
        }

        Assembly assembly = LoadAssembly(Path.Join(modDir, config.AssemblyFile));

        Type[] modTypes = assembly.GetExportedTypes().Where(type => type.IsSubclassOf(typeof(Mod)) && !type.IsAbstract).ToArray();
        switch (modTypes.Length)
        {
            case 0:
                throw new TypeLoadException($"Subclass of Mod not found in {config.AssemblyFile}");
            case > 1:
                throw new TypeLoadException($"More than one class is subclass of Mod in {config.AssemblyFile}");
        }

        Mod mod = ReflectionUtils.CreateInstance<Mod>(modTypes[0]);
        mod.Config = config;
        mod.assembly = assembly;
        string modContentPath = string.Concat(modDir, "Contant/");
        if (Directory.Exists(modContentPath))
        {
            AssetManager assets = new FileSystemAssetManager(modContentPath);
            mod.Assets = assets;
        }
    }

    public static Assembly LoadAssembly(string dllPath)
    {
        if (!File.Exists(dllPath)) throw new InvalidModConfigurationException(dllPath, $"Config contains a reference to .dll file which was not found:");

        byte[] dllBytes = File.ReadAllBytes(dllPath);
        
        string pdbPath = Path.ChangeExtension(dllPath, ".pdb");
        if (File.Exists(pdbPath))
        {
            byte[] pdbBytes = File.ReadAllBytes(pdbPath);
            return Assembly.Load(dllBytes, pdbBytes);
        }

        return Assembly.Load(dllBytes);
    }

    public static string GetModConfigPath(ReadOnlySpan<char> modDir) => $"{modDir}config.json";

    public static void PreLoadContent()
    {

    }
}
