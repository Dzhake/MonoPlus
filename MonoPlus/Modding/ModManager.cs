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
    public static List<ModID> LoadedModsIDs = new();
    public static Queue<ModConfig> QueuedModConfigs = new();
    public static string ModsDirectory = $"{AppContext.BaseDirectory}Mods/";

    /// <summary>
    /// Initializes mod manager, including loading all mods from <see cref="ModsDirectory"/>
    /// </summary>
    public static void Initialize()
    {
        foreach (ReadOnlySpan<char> modDir in Directory.EnumerateDirectories(ModsDirectory, "*", SearchOption.TopDirectoryOnly))
            LoadMod(modDir);
    }

    /// <summary>
    /// Loads mod from specified path
    /// </summary>
    /// <param name="modDir">Path to folder where mod is located</param>
    /// <exception cref="InvalidModConfigurationException">Thrown if mod configuration is invalid or not found</exception>
    /// <exception cref="TypeLoadException">Thrown if </exception>
    public static void LoadMod(ReadOnlySpan<char> modDir)
    {
        string configPath = GetModConfigPath(modDir);
        if (!File.Exists(configPath)) throw new InvalidModConfigurationException(configPath, "File not found");

        ModConfig config;
        Mod mod;

        //Load config
        using (var configStream = File.OpenRead(configPath))
        {
            config = JsonSerializer.Deserialize<ModConfig>(configStream) ?? throw new InvalidModConfigurationException(configPath, "Deserializer returned null.");
        }

        //Load mod assembly and find mod class or use Mod
        if (!string.IsNullOrEmpty(config.AssemblyFile))
        {
            Assembly assembly = LoadAssembly(Path.Join(modDir, config.AssemblyFile));
            mod = ReflectionUtils.CreateInstance<Mod>(FindModType(assembly));
            mod.Config = config;
            mod.assembly = assembly;
        }
        else
            mod = new(config);
        
        //Create content manager for mod
        string modContentPath = string.Concat(modDir, "Content");
        AssetManager? assets = ExternalAssetManagerBase.FolderOrZip(modContentPath);
        if (assets is not null)
        {
            mod.Assets = assets;
            Assets.Assets.RegisterAssetManager(assets, config.Name);
        }
    }

    private static Type FindModType(Assembly assembly)
    {
        Type[] modTypes = assembly.GetExportedTypes().Where(type => type.IsSubclassOf(typeof(Mod)) && !type.IsAbstract).ToArray();
        switch (modTypes.Length)
        {
            case 0:
                return typeof(Mod);
                //throw new TypeLoadException($"Subclass of Mod not found in {assembly.FullName}");
            case > 1:
                throw new TypeLoadException($"More than one class is subclass of Mod in {assembly.FullName}");
        }

        return modTypes[0];
    }

    /// <summary>
    /// Loads assembly and relevant .pdb file (if possible)
    /// </summary>
    /// <param name="dllPath">Path to file with assembly</param>
    /// <returns>Loaded assembly</returns>
    /// <exception cref="InvalidModConfigurationException">Thrown if assembly file doesn't exist</exception>
    public static Assembly LoadAssembly(string dllPath)
    {
        if (!File.Exists(dllPath)) throw new InvalidModConfigurationException(dllPath, "Config contains a reference to .dll file which was not found");

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
