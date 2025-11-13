using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MonoPlus.AssetsSystem;
using MonoPlus.SaveSystem;
using MonoPlus.Utils;
using MonoPlus.Utils.General;
using Serilog;

namespace MonoPlus.ModSystem;

/// <summary>
/// Class for loading, unloading and reloading <see cref="Mod"/>s.
/// </summary>
public static class ModManager
{
    /// <summary>
    /// <see cref="Dictionary{TKey,TValue}"/> of <see cref="Mod"/>s, where key is <see cref="Mod"/>'s name in it's <see cref="ModId"/>, and value is the <see cref="Mod"/> with that name. Null if <see cref="Initialized"/> is false.
    /// </summary>
    public static Dictionary<string, Mod>? Mods;

    /// <summary>
    /// Lock for the <see cref="Mods"/> dictionary.
    /// </summary>
    public static ReaderWriterLockSlim ModsLock = null!;

    
    /// <summary>
    /// <see cref="HashSet{T}"/> of enabled mods' names. Saves to "%Saves%/ModManager/ModDirsCache.json"
    /// </summary>
    public static HashSet<string> EnabledMods = null!;

    /// <summary>
    /// Cache for accessing mod directory name by mod name, as those might not match. Serialized to "%Saves%/ModManager/ModDirsCache.json".
    /// </summary>
    public static Dictionary<string, string> ModDirsCache = null!;

    /// <summary>
    /// <see cref="ModManagerConfig"/> defining <see cref="ModManager"/>'s behaviour. Serialized to "%Saves%/ModManager/Config.json".
    /// </summary>
    public static ModManagerConfig Config = null!; 
    
    /// <summary>
    /// Whether each value in EnabledMods has a key in ModNameToDirCache.
    /// </summary>
    public static bool FoundEnabledMods = true;

    /// <summary>
    /// Whether to load disabled mods even if <see cref="FoundEnabledMods"/> is <see langword="true"/>
    /// </summary>
    public static bool ForceLoadDisabledMods;

    /// <summary>
    /// Whether to load disabled mods too, or only enabled ones.
    /// </summary>
    public static bool LoadDisabledMods => !FoundEnabledMods || ForceLoadDisabledMods;
    

    /// <summary>
    /// <see cref="Directory"/> path from where the mods are loaded by default.
    /// </summary>
    public static readonly string ModsDirectory = $"{AppContext.BaseDirectory}Mods";

    
    /// <summary>
    /// Whether <see cref="Initialize"/> was called.
    /// </summary>
    public static bool Initialized;
    
    
    /// <summary>
    /// Whether <see cref="ModManager"/> currently loads or unloads some mods.
    /// </summary>
    public static bool InProgress => LoadedModsCount != TotalModsCount;

    /// <summary>
    /// Amount of successfully loaded mods, or mods which are about to load.
    /// </summary>
    public static int TotalModsCount;
    
    /// <summary>
    /// Amount of successfully loaded mods.
    /// </summary>
    public static int LoadedModsCount;

    /// <summary>
    /// Amount of mods which are currently waiting for their dependencies to load. After all dependencies for some mod are loaded, this value is decreased by one, and when the mod is loaded <see cref="LoadedModsCount"/> is increased by one.
    /// </summary>
    private static int WaitingModsCount;

    /// <summary>
    /// Whether some mods are stuck in infinite dependency-awaiting loop, and those mods should skip the loop and try to load or error.
    /// </summary>
    private static bool SkipLoading;


    /// <summary>
    /// Initializes mod manager, setting all static fields. Does nothing if <see cref="Initialized"/> is <see langword="true"/>.
    /// </summary>
    public static void Initialize()
    {
        if (Initialized) return;
        Initialized = true;
        Mods = new();
        ModsLock = new();
        EnabledMods = SaveManager.Load<HashSet<string>>(Path.Join(SaveManager.SavesLocation, "ModManager", "EnabledModsJson")) ?? new HashSet<string>();
        ModDirsCache = SaveManager.Load<Dictionary<string, string>>(Path.Join(SaveManager.SavesLocation, "ModManager", "ModDirsCache.json")) ?? new();
        Config = SaveManager.Load<ModManagerConfig>(Path.Join(SaveManager.SavesLocation, "ModManager", "Config.json")) ?? new();
        ForceLoadDisabledMods = Config.PreloadMods;
    }


    /// <summary>
    /// Updates all <see cref="Mods"/>.
    /// </summary>
    public static void Update()
    {
        if (Mods is null) return;

        foreach (Mod mod in Mods.Values)
            mod.Listener?.Update();
    }
    
    /// <summary>
    /// Loads all mods from <see cref="ModsDirectory"/>.
    /// </summary>
    public static void LoadMods()
    {
        string[] modDirs = new string[EnabledMods.Count];
        
        int i = 0;
        foreach (string enabledModName in EnabledMods)
        {
            if (!ModDirsCache.TryGetValue(enabledModName, out string? enabledModDir))
            {
                FoundEnabledMods = false;
                break;
            }

            modDirs[i] = enabledModDir;
            i++;
        }

        if (!FoundEnabledMods)
            LoadMods(ModsDirectory);
        LoadMods(modDirs);
    }
    
    /// <summary>
    /// Load all mods from specified directory.
    /// </summary>
    /// <param name="modsDir"><see cref="Directory"/> path to dir with other dirs, where each subdir contains config.json file.</param>
    public static void LoadMods(string modsDir)
    {
        string[] modDirs = Directory.EnumerateDirectories(modsDir, "*", SearchOption.TopDirectoryOnly).ToArray();
        LoadMods(modDirs);
    }

    /// <summary>
    /// Load all mods from the specified directories and run <see cref="PreventModLockAsync"/>. Each directory must contain config.json file.
    /// </summary>
    /// <param name="modDirs">Array of <see cref="Directory"/> paths to mods' dirs.</param>
    /// <exception cref="InvalidOperationException">ModManager is not Initialized.</exception>
    public static void LoadMods(string[] modDirs)
    {
        if (!Initialized) throw new InvalidOperationException("ModManager is not Initialized");
        
        SkipLoading = false;
        
        if (modDirs.Length == 0) return;
        TotalModsCount += modDirs.Length;
        Log.Information("Loading {ModsCount} mods", modDirs.Length);

        foreach (string modDir in modDirs)
            MainThread.Add(LoadModAsync(modDir));

        MainThread.Add(PreventModLockAsync());
    }

    /// <summary>
    /// Prevents mods from infinitely wait for their dependencies by checking if (WaitingModsCount + LoadedModsCount == TotalModsCount) or all mods are loaded each 1 millisecond. Exits when either condition is true.
    /// </summary>
    public static async Task PreventModLockAsync()
    {
        int tries = 0;
        const int maxTries = 3;
        int previousLoadedModsCount = LoadedModsCount;
        
        while (true)
        {
            await Task.Delay(1); //1ms delay between checks.
            
            //Technically if this is condition is true then second one is true too, but this is 'the good ending' and second condition is 'the bad ending'.
            
            if (!InProgress) //All mods are loaded
            {
                Log.Information("Finished loading mods");
                return;
            }

            if (WaitingModsCount + LoadedModsCount == TotalModsCount && previousLoadedModsCount == LoadedModsCount)
            {
                // All mod configs were loaded, and some of the configs wait. Maybe last mod successfully loaded mod loaded just now, and waiting mods didn't check dependencies yet, and actually one of them can load. Wait a few moments to be sure that no mod will load.    But what if a mod loads between these checks? The condition is still true, so this thread thinks that the mods are still stuck. Track loaded mods count, to check that no mods loaded between checks. If some did, then previousLoadedModsCount doesn't match LoadedModsCount, so the loading is not stuck and we reset tries. 
                if (tries < maxTries)
                {
                    tries++;
                    previousLoadedModsCount = LoadedModsCount;
                    continue;
                }

                SkipLoading = true;
                Log.Information("Skipping loading for {WaitingModsCount} mods", WaitingModsCount);
                return;
            }

            tries = 0;
            previousLoadedModsCount = LoadedModsCount;
        }
    }
    
    /// <summary>
    /// Load mod from the specified <paramref name="modDir"/> asynchronously. Requires <see cref="PreventModLockAsync"/> being active to run correctly.
    /// </summary>
    /// <param name="modDir"><see cref="Directory"/> path where mod is located.</param>
    /// <exception cref="Exception">ModManager.Mods is null</exception>
    public static async Task LoadModAsync(string modDir)
    {
        //Check that config exists.
        string configPath = GetModConfigPath(modDir);
        if (!File.Exists(configPath)) AddModToDict(FailedToLoadMod.New(new FileNotFoundException($"Could not find mod config at the specified path: {configPath}", configPath), Path.GetRelativePath(ModsDirectory, modDir)));

        //Load config.
        ModConfig? config = LoadModConfig(configPath);
        if (config is null) return; //invalid config, handled by LoadModConfig() .

        string modName = config.Id.Name;
        if (Mods is null) throw new Exception($"ModManager.Mods is null while loading mod: {modName}");

        try
        {
            ModsLock.EnterReadLock();
            if (Mods.TryGetValue(modName, out Mod? existingMod))
            {
                switch (existingMod.Status)
                {
                case ModStatus.Disabled:
                    Log.Warning("Trying to load mod that's already loaded and marked as Disabled. Mark it AwaitingLoad first, then load.");
                    return;
                case ModStatus.Enabled:
                    
                    return;
                case ModStatus.FailedToLoad:
                    break;
                
                default:
                    throw new IndexOutOfRangeException($"Status for mod {existingMod.Config.Id.Name} is not any valid value: {existingMod.Status} (while overwriting the mod)");
                }
            }
                
        }
        finally
        {
            ModsLock.ExitReadLock();
        }

        if (RemoveLoadedDeps(config))
        { //Config is ready to load.
            LoadModFromConfig(config, modDir);
            return;
        }

        Interlocked.Add(ref WaitingModsCount, 1);
        Log.Information("Delaying mod: {ModName}", config.Id.Name);
        int loadedModsCount = Mods.Count;
        
        while (true)
        {
            await Task.Delay(1);
            int newLoadedModsCount = Mods.Count; 
            if ((loadedModsCount < newLoadedModsCount || SkipLoading) && RemoveLoadedDeps(config)) //config is ready to load
                break;

            if (SkipLoading)
            {
                //All possible deps were just removed.
                if (config.HardDeps is not null && config.HardDeps.Count > 0)
                {
                    AddModToDict(FailedToLoadMod.HardDepsNotMet(config));
                    return;
                }

                //Some of soft deps failed to load. Don't care.
                Log.Debug("Some soft deps of {ModName} were not loaded", config.Id.Name);
                break;
            }

            loadedModsCount = newLoadedModsCount;
        }
        
        Interlocked.Add(ref WaitingModsCount, -1);
        LoadModFromConfig(config, modDir);
    }

    /// <summary>
    /// Loads a mod using info from the specified <paramref name="config"/>.
    /// </summary>
    /// <param name="config"><see cref="ModConfig"/> with all the information related to the mod.</param>
    /// <param name="modDir"><see cref="Directory"/> path where the mod is located.</param>
    public static void LoadModFromConfig(ModConfig config, string modDir)
    {
        Mod mod = new();
        mod.AssignConfig(config);
        mod.Directory = modDir;

        string contentDir = GetContentDirectory(modDir);
        if (Directory.Exists(contentDir))
        {
            mod.Assets = new FileAssetManager(contentDir);
            Assets.RegisterAssetManager(mod.Assets, mod.Config.Id.Name);
            mod.ContentType |= ModContentType.Assets;
        }
        
        TryLoadAssembly(mod);
        AddModToDict(mod);

        Interlocked.Add(ref LoadedModsCount, 1);
        Log.Information("Loaded mod: {ModName}", config.Id.Name);
    }

    
    /// <summary>
    /// Returns config <see cref="File"/> path for that mod.
    /// </summary>
    /// <param name="modDir">Directory, where "config.json" is located.</param>
    /// <returns><see cref="File"/> path to "config.json".</returns>
    [Pure]
    public static string GetModConfigPath(string modDir) => Path.Join(modDir, "config.json");

    /// <summary>
    /// Get <see cref="Directory"/> path for the specified <paramref name="modDir"/>.
    /// </summary>
    /// <param name="modDir"><see cref="Directory"/> path with the mod.</param>
    /// <returns><see cref="Directory"/> path to the mod's content directory.</returns>
    [Pure]
    public static string GetContentDirectory(string modDir) => Path.Join(modDir, "Content");
    

    /// <summary>
    /// Loads <see cref="ModConfig"/> from specified path.
    /// </summary>
    /// <param name="configPath"><see cref="File"/> path to config.</param>
    /// <returns><see cref="ModConfig"/> parsed from that <see cref="File"/>.</returns>
    /// <exception cref="InvalidModConfigException">Thrown if <see cref="ModConfig"/> couldn't be deserialized.</exception>
    public static ModConfig? LoadModConfig(string configPath)
    {
        using var configStream = File.OpenRead(configPath);
        try
        {
            ModConfig? result = JsonSerializer.Deserialize<ModConfig>(configStream, Json.Common);
            if (result is not null) return result;
            AddModToDict(FailedToLoadMod.ConfigDeserializeNull(configPath, Path.GetFileName(Path.GetDirectoryName(configPath) ?? ""))); //invalid mod config
            return null;
        }
        catch (Exception exception)
        {
            AddModToDict(FailedToLoadMod.New(new JsonException($"Got exception from deserializer with file at \"{configPath}\":\n {exception}"), Path.GetFileName(Path.GetDirectoryName(configPath) ?? ""))); //invalid mod config
            return null;
        }
    }

    /// <summary>
    /// Removes all dependencies which are fully loaded from the specified <paramref name="config"/>'s <see cref="ModConfig.HardDeps"/> and <see cref="ModConfig.SoftDeps"/>, and returns whether all deps for the specified <paramref name="config"/> are loaded.
    /// </summary>
    /// <param name="config">Config whose deps to remove.</param>
    /// <returns>Whether all deps for the specified <paramref name="config"/> are loaded.</returns>
    public static bool RemoveLoadedDeps(ModConfig config) => RemoveLoadedDeps(config.HardDeps) && RemoveLoadedDeps(config.SoftDeps);

    /// <summary>
    /// Removes all dependencies which are fully loaded from the specified <paramref name="deps"/>, and returns whether all deps are loaded.
    /// </summary>
    /// <param name="deps"><see cref="List{ModDep}"/> of <see cref="ModDep"/>s to check and modify.</param>
    /// <returns>Whether all deps are loaded.</returns>
    /// <exception cref="InvalidOperationException">ModManager.Mods is null.</exception>
    public static bool RemoveLoadedDeps(List<ModDep>? deps)
    {
        if (deps is null) return true;
        if (Mods is null) throw new InvalidOperationException("ModManager.Mods is null");

        foreach (Mod loadedMod in Mods.Values)
        {
            ModId loadedModId = loadedMod.Config.Id;
            for (int i = 0; i < deps.Count; i++)
            {
                ModDep dep = deps[i];
                if (loadedModId.Matches(dep)) deps.RemoveAt(i);
            }

            if (deps.Count == 0) return true;
        }

        return false;
    }

    /// <summary>
    /// Adds the mod to <see cref="Mods"/> in a thread-safe way.
    /// </summary>
    /// <param name="mod">Mod to add.</param>
    /// <exception cref="InvalidOperationException"><see cref="Mods"/> is null.</exception>
    public static void AddModToDict(Mod mod)
    {
        if (Mods is null) throw new InvalidOperationException("ModManager.Mods is null");

        try
        {
            ModsLock.EnterWriteLock();
            Mods.TryAdd(mod.Config.Id.Name, mod);
        }
        finally
        {
            ModsLock.ExitWriteLock();
        }
    }
    
    /// <summary>
    /// Tries to load assembly for the mod. Does nothing if mod doesn't have an assembly.
    /// </summary>
    /// <param name="mod">Mod with successfully loaded config.</param>
    public static void TryLoadAssembly(Mod mod)
    {
        ModConfig config = mod.Config;
        if (string.IsNullOrEmpty(config.AssemblyFile)) return;
        string dllPath = Path.Join(mod.Directory, config.AssemblyFile);
        Log.Information("Loading assembly from {DllPath}", dllPath);
        ModAssemblyLoadContext assemblyContext = LoadAssembly(mod, dllPath);
        mod.AssemblyContext = assemblyContext;
        mod.Listener = ReflectionUtils.CreateInstance<ModListener>(FindModListenerType(assemblyContext.Assemblies.ElementAt(0)));
        mod.Listener.mod = mod;
        mod.Listener.Initialize();
    }

    /// <summary>
    /// Loads <see cref="Assembly"/> at <paramref name="dllPath"/>, sets <see cref="Mod.AssemblyContext"/>, and returns <see cref="ModAssemblyLoadContext"/> for the loaded assembly.
    /// </summary>
    /// <param name="mod"><see cref="Mod"/> related to the assembly.</param>
    /// <param name="dllPath"><see cref="File"/> path to .dll file with valid dotnet <see cref="Assembly"/></param>
    private static ModAssemblyLoadContext LoadAssembly(Mod mod, string dllPath)
    {
        ModAssemblyLoadContext assemblyContext = new(mod);
        using FileStream assemblyFileStream = new(dllPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        assemblyContext.LoadFromStream(assemblyFileStream);
        mod.AssemblyContext = assemblyContext;
        return assemblyContext;
    }

    /// <summary>
    /// Finds type derived from <see cref="ModListener"/> in the specified <paramref name="assembly"/>, and returns it.
    /// </summary>
    /// <param name="assembly"><see cref="Assembly"/>, where to find the type.</param>
    /// <returns>Type derived from <see cref="ModListener"/> in the specified assembly</returns>
    /// <exception cref="TypeLoadException">The specified <paramref name="assembly"/> contains 0 or more than 1 type derived from <see cref="ModListener"/>.</exception>
    public static Type FindModListenerType(Assembly assembly)
    {
        Type[] modTypes = assembly.GetExportedTypes().Where(type => type.IsSubclassOf(typeof(ModListener)) && !type.IsAbstract).ToArray();
        return modTypes.Length switch
        {
            0 => throw new TypeLoadException($"Subclass of ModListener not found in {assembly.FullName}"),
            > 1 => throw new TypeLoadException($"Found more than one subclass of ModListener in {assembly.FullName}"),
            _ => modTypes[0]
        };
    }
    
    
    
    

    /// <summary>
    /// Reloads assembly for the specified <paramref name="mod"/> by adding <see cref="ReloadAssemblyAsync"/> as task for the <see cref="MainThread"/>.
    /// </summary>
    /// <param name="mod">Mod whose assembly to reload.</param>
    public static void ReloadAssembly(Mod mod) => MainThread.Add(ReloadAssemblyAsync(mod));

    /// <summary>
    /// Reloads assembly for the specified <paramref name="mod"/> asynchronously. See <see cref="ReloadAssembly"/> wrapper to set this as task for the <see cref="MainThread"/>. 
    /// </summary>
    /// <param name="mod">Mod whose assembly to reload.</param>
    public static async Task ReloadAssemblyAsync(Mod mod)
    {
        if (!MonoPlusMain.HotReload)
        {
            Log.Warning("Tried to reload assembly of {ModName}, but hot reload is off", mod.Config.Id.Name);
            return;
        }

        if ((mod.ContentType & ModContentType.Code) != ModContentType.Code)
        {
            Log.Warning("Tried to reload assembly of {ModName}, but mod.ContentType doesn't include ModContentType.Code", mod.Config.Id.Name);
            return;
        }

        Interlocked.Add(ref LoadedModsCount, -1);
        Log.Information("Reloading mod: {ModName}", mod.Config.Id.Name);
        await UnloadAssemblyAsync(mod);
        TryLoadAssembly(mod);

        Interlocked.Add(ref LoadedModsCount, 1);
    }

    /// <summary>
    /// Unloads assembly context for the specified <paramref name="mod"/>.
    /// </summary>
    /// <param name="mod">Mod with assembly context to unload.</param>
    private static async Task UnloadAssemblyAsync(Mod mod)
    {
        if (mod.AssemblyContext is null) throw new Exception($"Tried to unload assembly for {mod.Config.Id.Name}, but mod.AssemblyContext is null");
        
        Log.Information("Unloading assembly for: {ModName}", mod.Config.Id.Name);
        
        mod.HarmonyInstance?.UnpatchSelf();
        WeakReference alcWeakReference = new(mod.AssemblyContext, trackResurrection: true);
        mod.AssemblyContext?.Dispose();
        mod.Listener = null;

        //Wait until assembly is unloaded
        while (alcWeakReference.IsAlive)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            await Task.Delay(1);
        }

        Log.Information("Assembly unloaded for: {ModName}", mod.Config.Id.Name);
    }


    /*public static void ReloadModConfig(Mod mod)
    {
        mod.Config = LoadModConfig(GetModConfigPath(mod.Directory));
    }*/
    

    
}