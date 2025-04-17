using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using MonoPlus.AssetsManagment;
using Serilog;

namespace MonoPlus.Modding;

/// <summary>
/// Manages loading and updating <see cref="Mod"/>s
/// </summary>
public static class ModManager
{
    /// <summary>
    /// If true, all <see cref="AssetManager"/>s created for mods try to preload assets
    /// </summary>
    public static bool PreloadAssets = true;

    /// <summary>
    /// <see cref="List{Mod}"/> of fully loaded and PreInitialized <see cref="Mod"/>s
    /// </summary>
    public static List<Mod> LoadedMods = new();

    /// <summary>
    /// <see cref="Queue{ModConfig}"/> of <see cref="QueuedModConfigs"/>s, which are not yet ready to be loaded
    /// </summary>
    public static Queue<DelayedConfigInfo> QueuedModConfigs = new();

    /// <summary>
    /// Directory where mods should be installed
    /// </summary>
    public static string ModsDirectory = $"{AppContext.BaseDirectory}Mods/";

    /// <summary>
    /// Initializes mod manager, including loading all mods from <see cref="ModsDirectory"/>
    /// </summary>
    public static void Initialize()
    {
        foreach (ReadOnlySpan<char> modDir in Directory.EnumerateDirectories(ModsDirectory, "*", SearchOption.TopDirectoryOnly))
            LoadMod(modDir);

        PostLoadMods();
    }


    /// <summary>
    /// Loads mod from specified path
    /// </summary>
    /// <param name="modDir">Path to folder where mod is located</param>
    /// <exception cref="InvalidModConfigurationException">Thrown if mod configuration is invalid or not found</exception>
    /// <exception cref="TypeLoadException">Thrown if </exception>
    public static Mod? LoadMod(ReadOnlySpan<char> modDir)
    {
        //Check that config exists
        string configPath = GetModConfigPath(modDir);
        if (!File.Exists(configPath)) throw new InvalidModConfigurationException(configPath, "File not found");

        //Load config
        ModConfig config = LoadModConfig(configPath);

        //Delay if mod has dependencies
        if (config.Dependencies is not null)
        {
            Log.Information("{ModName} has dependencies, delaying..", config.ID.Name);
            QueuedModConfigs.Enqueue(new(config, modDir.ToString()));
            return null;
        }
        
        return LoadModFromConfig(modDir, config);
    }

    private static Mod LoadModFromConfig(ReadOnlySpan<char> modDir, ModConfig config)
    {
        Mod mod;
        //Load mod assembly and find mod class or use Mod
        if (!string.IsNullOrEmpty(config.AssemblyFile))
        {
            string dllPath = Path.Join(modDir, config.AssemblyFile);
            Log.Information("Loading assembly from {DllPath}..", dllPath);
            ModAssemblyLoadContext assemblyContext = new();
            assemblyContext.LoadFromAssemblyPath(dllPath);
            mod = ReflectionUtils.CreateInstance<Mod>(FindModType(assemblyContext.Assemblies.ElementAt(0)));
            mod.Config = config;
            mod.AssemblyContext = assemblyContext;
        }
        else
            mod = new(config);
        
        //Create asset manager for mod
        string modContentPath = string.Concat(modDir, "Content");
        AssetManager? assets = ExternalAssetManagerBase.FolderOrZip(modContentPath);
        if (assets is not null)
        {
            mod.Assets = assets;
            Assets.RegisterAssetManager(assets, config.ID.Name);
            if (PreloadAssets) assets.PreloadAssets();
        }

        LoadedMods.Add(mod);

        return mod;
    }

    public static void UnloadMod(Mod mod)
    {
        LoadedMods.Remove(mod);
        mod.AssemblyContext?.Dispose();
    }

    private static void PostLoadMods()
    {
        foreach (DelayedConfigInfo delayedConfig in QueuedModConfigs)
        {
            if (!DependenciesMet(delayedConfig.Config))
            {
                Log.Warning("Couldn't load dependencies for {ModName}, skipping..", delayedConfig.Config.ID.Name);
                continue;
            }

            LoadModFromConfig(delayedConfig.ModDir, delayedConfig.Config);
        }
    }

    /// <summary>
    /// Checks if all dependencies for specific <see cref="ModConfig"/> were loaded
    /// </summary>
    /// <param name="config">Config to check</param>
    /// <returns><see langword="true"/> if dependencies were loaded, <see langword="false"/> otherwise</returns>
    public static bool DependenciesMet(ModConfig config)
    {
        if (config.Dependencies is null) return true;
        List<ModDep> deps = config.Dependencies;

        foreach (Mod loadedMod in LoadedMods)
        {
            ModID loadedModID = loadedMod.Config.ID;
            for (int i = 0; i < deps.Count; i++)
            {
                ModDep dep = deps[i];
                if (loadedModID.Matches(dep)) deps.RemoveAt(i);
            }

            if (config.Dependencies.Count == 0) return true;
        }

        return false;
    }

    /// <summary>
    /// Loads <see cref="ModConfig"/> from specified path
    /// </summary>
    /// <param name="configPath">Path to config file</param>
    /// <returns><see cref="ModConfig"/> parsed from that file</returns>
    /// <exception cref="InvalidModConfigurationException">Thrown if <see cref="ModConfig"/> couldn't be deseralized</exception>
    public static ModConfig LoadModConfig(string configPath)
    {
        //Load config
        using var configStream = File.OpenRead(configPath);
        return JsonSerializer.Deserialize<ModConfig>(configStream) ?? throw new InvalidModConfigurationException(configPath, "Deserializer returned null.");
    }

    /// <summary>
    /// Finds type which inherits from <see cref="Mod"/> in given assembly, or <see cref="Mod"/> if not found
    /// </summary>
    /// <param name="assembly"><see cref="Assembly"/> which should contain 0 or 1 classes which inherit from <see cref="Mod"/></param>
    /// <returns>Type, which inherits <see cref="Mod"/> or typeof(Mod)</returns>
    /// <exception cref="TypeLoadException">Thrown if <see cref="assembly"/> contains more than 1 class which inherits <see cref="Mod"/></exception>
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

    /// <summary>
    /// Returns config file path for that mod
    /// </summary>
    /// <param name="modDir">Directory, where "config.json" is located</param>
    /// <returns>File path to config.json</returns>
    public static string GetModConfigPath(ReadOnlySpan<char> modDir) => $"{modDir}config.json";

    
}
