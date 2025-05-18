using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MonoPlus.Modding;

/// <summary>
/// Manages loading and updating <see cref="Mod"/>s
/// </summary>
public static class ModManager
{
    /// <summary>
    /// <see cref="Dictionary{TKey,TValue}"/> of <see cref="Mod"/>s, where key is <see cref="Mod"/>'s name in it's <see cref="ModId"/>, and value is the <see cref="Mod"/> with that name.
    /// </summary>
    public static readonly Dictionary<string, Mod> Mods = new();

    /// <summary>
    /// Directory where mods should be installed
    /// </summary>
    public static string ModsDirectory = $"{AppContext.BaseDirectory}Mods{Path.DirectorySeparatorChar}";

    /// <summary>
    /// Amount of successfully loaded mods
    /// </summary>
    public static int LoadedModsCount => ModManager.Mods.Count;

    /// <summary>
    /// Amount of successfully loaded mods, or mods which are about to load
    /// </summary>
    public static int TotalModsCount;

    /// <summary>
    /// Initializes mod manager, including loading all mods from <see cref="ModsDirectory"/>
    /// </summary>
    public static void Initialize()
    {
        ModLoader.LoadMods(ModsDirectory);
    }

    /// <summary>
    /// Updates all <see cref="Mods"/>
    /// </summary>
    public static void Update()
    {
        for (int i = 0; i < ModLoader.ModReloadTasks.Count; i++)
        {
            Task modReloadTask = ModLoader.ModReloadTasks[i];

            //if task crashed, then throw the error
            modReloadTask.Exception?.Handle(_ =>
                false); //Don't handle anything, it'll throw itself and error handler should catch it. TODO check if there's any other way to do this.

            //otherwise just remove it from the list.
            if (modReloadTask.IsCompleted)
            {
                ModLoader.ModReloadTasks.RemoveAt(i);
                i--;
            }
        }

        foreach (Mod mod in Mods.Values)
            mod.Update();
    }
}
