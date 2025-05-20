using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using MonoPlus.AssetsManagement;
using MonoPlus.Utils;
using Serilog;

namespace MonoPlus.Modding;

/// <summary>
/// Manages loading and unloading of <see cref="Mod"/>s
/// </summary>
public static class ModLoader
{

    /// <summary>
    /// List of tasks which reload mods. If Count is higher than 0, then some mods are currently reloaded.
    /// </summary>
    public static List<ModReloadTask> ModReloadTasks = new();

    /// <summary>
    /// Whether any mods were successfully fully loaded, to check in <see cref="ModManager.Mods"/> too, when sorting mods by dependencies.
    /// </summary>
    public static bool loadedAnyMods;


    /// <summary>
    /// Loads all mods from specified directory
    /// </summary>
    /// <param name="modsDir"><see cref="Directory"/> path to <see cref="Directory"/> with Directories, where each folder contains config.json</param>
    public static void LoadMods(string modsDir)
    {
        IEnumerable<string> modDirs = Directory.EnumerateDirectories(modsDir, "*", SearchOption.TopDirectoryOnly);
        List<ModConfig> configs = LoadModConfigs(modDirs);
        configs = ModConfigSorter.SortModConfigs(configs);

        ModManager.TotalModsCount += configs.Count;
        Log.Information("Loading {Count} mods", configs.Count);

        foreach (ModConfig config in configs)
            LoadModFromConfig(config);

        PostLoadMods(configs);
    }

    /// <summary>
    /// Loads mod from specified path
    /// </summary>
    /// <param name="modDirs"><see cref="IEnumerable{T}"/> of <see cref="Directory"/> paths, where each path is a directory with a mod.</param>
    public static List<ModConfig> LoadModConfigs(IEnumerable<string> modDirs)
    {
        List<ModConfig> configs = new();
        foreach (string modDir in modDirs)
            configs.Add(LoadModConfigFromFolder(modDir));
        return configs;
    }

    /// <summary>
    /// Loads <see cref="ModConfig"/> from <paramref name="modDir"/>
    /// </summary>
    /// <param name="modDir">Directory path where config.json is located</param>
    /// <returns>Loaded config</returns>
    /// <exception cref="InvalidModConfigurationException">Thrown if config file was not found</exception>
    public static ModConfig LoadModConfigFromFolder(string modDir)
    {
        //Check that config exists
        string configPath = GetModConfigPath(modDir);
        if (!File.Exists(configPath)) throw new InvalidModConfigurationException(configPath, "File not found");

        //Load config
        ModConfig config = LoadModConfig(configPath);
        config.ModDirectory = modDir;
        return config;
    }


    /// <summary>
    /// Loads <see cref="ModConfig"/> from specified path.
    /// </summary>
    /// <param name="configPath"><see cref="File"/> path to config.</param>
    /// <returns><see cref="ModConfig"/> parsed from that <see cref="File"/>.</returns>
    /// <exception cref="InvalidModConfigurationException">Thrown if <see cref="ModConfig"/> couldn't be deseralized.</exception>
    public static ModConfig LoadModConfig(string configPath)
    {
        //Load config
        using var configStream = File.OpenRead(configPath);
        try
        {
            return JsonSerializer.Deserialize<ModConfig>(configStream, Json.Common)
                   ?? throw new InvalidModConfigurationException(configPath, "Deserializer returned null.");
        }
        catch (Exception exception)
        {
            throw new JsonException($"Got exception from deserializer while deserializing \"{configPath}\":\n {exception}");
        }
    }
    
    /// <summary>
    /// Returns config <see cref="File"/> path for that mod
    /// </summary>
    /// <param name="modDir">Directory, where "config.json" is located</param>
    /// <returns><see cref="File"/> path to config.json</returns>
    [Pure] public static string GetModConfigPath(ReadOnlySpan<char> modDir) => Path.Join(modDir, "config.json");


    
    /// <summary>
    /// Loads <see cref="Mod"/> from already loaded <see cref="ModConfig"/>
    /// </summary>
    /// <param name="config"><see cref="ModConfig"/> with info related to mod</param>
    /// <param name="tryCreateAssetManager">Whether should try to create and register asset manager at "<paramref name="config.ModDirectory"/>/Content".</param>
    private static void LoadModFromConfig(ModConfig config, bool tryCreateAssetManager = true)
    {
        Log.Information("Loading mod: {ModName}", config.Id.Name);
        Mod mod = LoadModAssemblyAndGetMod(config.ModDirectory, config);

        if (tryCreateAssetManager)
        {
            //Create asset manager for mod
            string modContentPath = Path.Combine(config.ModDirectory, "Content");
            AssetManager? assets = ExternalAssetManagerBase.FolderOrZip(modContentPath);
            if (assets is not null)
            {
                mod.Assets = assets;
                Assets.RegisterAssetManager(assets, config.Id.Name);
            }
        }

        ModManager.Mods.Add(mod.Config.Id.Name, mod);
        mod.Initialize();
        Log.Information("Succesfully loaded mod: {ModName}", config.Id.Name);
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

        foreach (Mod loadedMod in ModManager.Mods.Values)
        {
            ModId loadedModId = loadedMod.Config.Id;
            for (int i = 0; i < deps.Count; i++)
            {
                ModDep dep = deps[i];
                if (loadedModId.Matches(dep)) deps.RemoveAt(i);
            }

            if (config.Dependencies.Count == 0) return true;
        }

        return false;
    }


    /// <summary>
    /// Loads assembly from <see cref="ModConfig.AssemblyFile"/>, and tries to <see cref="FindModType"/> in it. If <see cref="ModConfig.AssemblyFile"/> is null or empty, returns <see langword="new"/> <see cref="Mod"/>() instead. Also calls <see cref="Mod.AssignConfig"/>.
    /// </summary>
    /// <param name="modDir"><see cref="Mod"/>'s <see cref="Directory"/> path</param>
    /// <param name="config"><see cref="ModConfig"/> loaded from config.json found in <paramref name="modDir"/></param>
    /// <returns>Subclass of <see cref="Mod"/> from <see cref="Mod"/>'s <see cref="Assembly"/>, or <see cref="Mod"/></returns>
    public static Mod LoadModAssemblyAndGetMod(ReadOnlySpan<char> modDir, ModConfig config)
    {
        Mod mod;
        //Load mod assembly and find mod class or use Mod
        if (!string.IsNullOrEmpty(config.AssemblyFile))
        {
            string dllPath = Path.Join(modDir, config.AssemblyFile);
            Log.Information("Loading assembly from {DllPath}", dllPath);
            ModAssemblyLoadContext assemblyContext = LoadAssembly(config, dllPath);
            mod = ReflectionUtils.CreateInstance<Mod>(FindModType(assemblyContext.Assemblies.ElementAt(0)));
            mod.AssemblyContext = assemblyContext;
        }
        else
            mod = new();
        
        mod.AssignConfig(config);

        return mod;
    }

    /// <summary>
    /// Loads <see cref="Assembly"/> at <paramref name="dllPath"/>, and returns <see cref="ModAssemblyLoadContext"/> with it
    /// </summary>
    /// <param name="config"><see cref="ModConfig"/> used for <see cref="ModAssemblyLoadContext"/></param>
    /// <param name="dllPath"><see cref="File"/> path to .dll file with valid dotnet <see cref="Assembly"/></param>
    public static ModAssemblyLoadContext LoadAssembly(ModConfig config, string dllPath)
    {
        ModAssemblyLoadContext assemblyContext = new(config);
        FileStream assemblyFileStream = new(dllPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        assemblyContext.LoadFromStream(assemblyFileStream);
        assemblyFileStream.Close();
        return assemblyContext;
    }
    
    /// <summary>
    /// Finds type which inherits from <see cref="Mod"/> in given assembly, or <see cref="Mod"/> if not found
    /// </summary>
    /// <param name="assembly"><see cref="Assembly"/> which should contain 0 or 1 classes which inherit from <see cref="Mod"/></param>
    /// <returns>Type, which inherits <see cref="Mod"/> or is <see langword="typeof"/>(<see cref="Mod"/>)</returns>
    /// <exception cref="TypeLoadException">Thrown if <paramref name="assembly"/> contains more than 1 class which inherits <see cref="Mod"/></exception>
    public static Type FindModType(Assembly assembly)
    {
        Type[] modTypes = assembly.GetExportedTypes().Where(type => type.IsSubclassOf(typeof(Mod)) && !type.IsAbstract).ToArray();
        return modTypes.Length switch
        {
            0 => typeof(Mod),  //throw new TypeLoadException($"Subclass of Mod not found in {assembly.FullName}");
            > 1 => throw new TypeLoadException($"More than one class is subclass of Mod in {assembly.FullName}"),
            _ => modTypes[0]
        };
    }

    /// <summary>
    /// Starts reloading mod related to the <paramref name="config"/>, including <see cref="ModAssemblyLoadContext"/> but excluding it's <see cref="AssetManager"/>
    /// </summary>
    /// <param name="config">Config related to mod</param>
    public static void ReloadMod(ModConfig config)
    {
        ModReloadTasks.Add(new(ReloadModAsync(config), config));
    }

    /// <summary>
    /// Reloads mod related to the <paramref name="config"/>, including <see cref="ModAssemblyLoadContext"/> but excluding it's <see cref="AssetManager"/>
    /// </summary>
    /// <param name="config">Config related to mod</param>
    /// <returns>Mod reload task, which should be stored in <see cref="ModReloadTasks"/></returns>
    private static async Task ReloadModAsync(ModConfig config)
    {
        if (config.mod is null) throw new InvalidOperationException("Tried to reload mod, but config.mod is null");
        Log.Information("Reloading mod: {ModName}", config.Id.Name);
        AssetManager? assets = config.mod.Assets;
        await UnloadMod(config);
        LoadModFromConfig(config, false);
        config.mod.Assets = assets;
    }

    /// <summary>
    /// Unloads <see cref="Mod"/> related to specified <paramref name="config"/>, and doesn't return until it's <see cref="ModAssemblyLoadContext"/> is fully unloaded
    /// </summary>
    /// <param name="config"><see cref="ModConfig"/> related to <see cref="Mod"/> which should be unloaded</param>
    /// <returns>Mod unload task</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static async Task UnloadMod(ModConfig config)
    {
        Mod? mod = config.mod;
        string modName = config.Id.Name;
        if (mod is null) throw new InvalidOperationException("Trying to unload mod by config, but config.mod is null");
        Log.Information("Unloading mod: {ModName}", modName);
        mod.HarmonyInstance.UnpatchSelf();
        WeakReference alcWeakReference = new(mod.AssemblyContext, trackResurrection: true);
        mod.AssemblyContext?.Dispose();
        ModManager.Mods.Remove(modName);


        //Remove references to the Mod because it might be type from that assembly. These are required, do not remove.
        mod = null;
        config.mod = null;

        //Wait until assembly is unloaded
        while (alcWeakReference.IsAlive)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            await Task.Delay(1);
        }
        Log.Information("Assembly unloaded for: {ModName}", modName);
    }

    /// <summary>
    /// Calls <see cref="Mod.PostInitialize"/> for ALL loaded mods, and sets <see cref="loadedAnyMods"/>
    /// </summary>
    private static void PostLoadMods(List<ModConfig> configs)
    {
        foreach (ModConfig config in configs)
            config.mod?.PostInitialize();   
        
        loadedAnyMods = true;
    }
}
