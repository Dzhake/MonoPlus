﻿using Serilog;
using System.Reflection;
using System;
using System.Linq;
using MonoPlus.AssetsManagment;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace MonoPlus.Modding;

/// <summary>
/// Manages loading and unloading of <see cref="Mod"/>s
/// </summary>
public static class ModLoader
{
    /// <summary>
    /// <see cref="Queue{ModConfig}"/> of <see cref="DelayedModConfigs"/>s, which are not yet ready to be loaded
    /// </summary>
    public static Queue<ModConfig> DelayedModConfigs = new();


    /// <summary>
    /// Loads all mods from specified directory
    /// </summary>
    /// <param name="modsDir"><see cref="Directory"/> path to <see cref="Directory"/> with Directories, where each folder contains config.json</param>
    public static void LoadMods(string modsDir)
    {
        foreach (string modDir in Directory.EnumerateDirectories(modsDir, "*", SearchOption.TopDirectoryOnly))
            LoadMod(modDir);
        PostLoadMods();
    }

    /// <summary>
    /// Loads mod from specified path
    /// </summary>
    /// <param name="modDir">Path to folder where mod is located</param>
    /// <exception cref="InvalidModConfigurationException">Thrown if mod configuration is invalid or not found</exception>
    /// <exception cref="TypeLoadException">Thrown if </exception>
    public static Mod? LoadMod(string modDir)
    {
        //Check that config exists
        string configPath = GetModConfigPath(modDir);
        if (!File.Exists(configPath)) throw new InvalidModConfigurationException(configPath, "File not found");

        //Load config
        ModConfig config = LoadModConfig(configPath);
        config.ModDirectory = modDir;

        //Delay if mod has dependencies
        if (config.Dependencies is not null)
        {
            Log.Information("{ModName} has dependencies, delaying..", config.ID.Name);
            DelayedModConfigs.Enqueue(config);
            return null;
        }
        
        return LoadModFromConfig(config);
    }



    /// <summary>
    /// Loads <see cref="ModConfig"/> from specified path
    /// </summary>
    /// <param name="configPath">Path to config <see cref="File"/></param>
    /// <returns><see cref="ModConfig"/> parsed from that <see cref="File"/></returns>
    /// <exception cref="InvalidModConfigurationException">Thrown if <see cref="ModConfig"/> couldn't be deseralized</exception>
    public static ModConfig LoadModConfig(string configPath)
    {
        //Load config
        using var configStream = File.OpenRead(configPath);
        return JsonSerializer.Deserialize<ModConfig>(configStream) ?? throw new InvalidModConfigurationException(configPath, "Deserializer returned null.");
    }
    
    /// <summary>
    /// Returns config <see cref="File"/> path for that mod
    /// </summary>
    /// <param name="modDir">Directory, where "config.json" is located</param>
    /// <returns><see cref="File"/> path to config.json</returns>
    [Pure] public static string GetModConfigPath(ReadOnlySpan<char> modDir) => $"{modDir}config.json";

    /// <summary>
    /// Loads <see cref="Mod"/> from already loaded <see cref="ModConfig"/>
    /// </summary>
    /// <param name="config"><see cref="ModConfig"/> with info related to mod</param>
    /// <returns></returns>
    private static Mod LoadModFromConfig(ModConfig config)
    {
        Mod mod = LoadModAssemblyAndGetMod(config.ModDirectory, config);

        //Create asset manager for mod
        string modContentPath = string.Concat(config.ModDirectory, "Content");
        AssetManager? assets = ExternalAssetManagerBase.FolderOrZip(modContentPath);
        if (assets is not null)
        {
            mod.Assets = assets;
            Assets.RegisterAssetManager(assets, config.ID.Name);
            assets.PreloadAssets();
        }

        ModManager.Mods.Add(mod.Config.ID.Name, mod);
        Log.Information("Loaded {ModName}", config.ID.Name);

        return mod;
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
            Log.Information("Loading assembly from {DllPath}..", dllPath);
            ModAssemblyLoadContext assemblyContext = LoadAssembly(dllPath);
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
    /// <param name="dllPath"><see cref="File"/> path to .dll file with valid dotnet <see cref="Assembly"/></param>
    public static ModAssemblyLoadContext LoadAssembly(string dllPath)
    {
        ModAssemblyLoadContext assemblyContext = new();
        assemblyContext.LoadFromAssemblyPath(dllPath);
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
    /// Reloads specified <paramref name="mod"/>, including all it's content and <see cref="ModAssemblyLoadContext"/>
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public static async Task ReloadMod(ModConfig config) {
        Log.Information("Reloading mod: {ModName}", config.ID.Name);
        await UnloadMod(config);
        LoadModFromConfig(config);
    }

    /// <summary>
    /// Unloads <see cref="Mod"/> related to specified <paramref name="config"/>, and doesn't return until it's <see cref="ModAssemblyLoadContext"/> is fully unloaded
    /// </summary>
    /// <param name="config"><see cref="ModConfig"/> related to <see cref="Mod"/> which should be unloaded</param>
    /// <returns>uh   idk</returns>
    public  static async Task UnloadMod(ModConfig config)
    {
        Mod? mod = config.mod;
        if (mod is null) throw new InvalidOperationException("Trying to unload mod by config, but config.mod is null!");
        Log.Information("Unloading mod: {ModName}", config.ID.Name);
        mod.HarmonyInstance?.UnpatchSelf();
        WeakReference alcWeakReference = new(mod.AssemblyContext, trackResurrection: true);
        mod.AssemblyContext?.Dispose();
        ModManager.Mods.Remove(mod.Config.ID.Name);

        //Remove references to the Mod because it might be type from that assembly
        mod = null;
        config.mod = null;

        //Wait until assembly is unloaded
        while (alcWeakReference.IsAlive) await Task.Delay(1);
        Log.Information("Assembly unloaded");
    }


    private static void PostLoadMods()
    {
        foreach (ModConfig config in DelayedModConfigs)
        {
            if (!DependenciesMet(config))
            {
                Log.Warning("Couldn't load dependencies for {ModName}, skipping..", config.ID.Name);
                continue;
            }

            LoadModFromConfig(config);
        }
    }
}
