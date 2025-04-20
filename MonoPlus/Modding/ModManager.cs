using System;
using System.Collections.Generic;

namespace MonoPlus.Modding;

/// <summary>
/// Manages loading and updating <see cref="Mod"/>s
/// </summary>
public static class ModManager
{
    /// <summary>
    /// <see cref="Dictionary{TKey,TValue}"/> of <see cref="Mod"/>s, where key is <see cref="Mod"/>'s name in it's <see cref="ModID"/>, and value is the <see cref="Mod"/> with that name.
    /// </summary>
    public static readonly Dictionary<string, Mod> Mods = new();

    /// <summary>
    /// Directory where mods should be installed
    /// </summary>
    public static string ModsDirectory = $"{AppContext.BaseDirectory}Mods/";

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
        foreach (Mod mod in Mods.Values)
            mod.Update();
    }
}
