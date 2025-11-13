using System.Collections.Generic;
using System.Threading;
using MonoPlus.ModSystem;

namespace MonoPlus.ModdingNew;

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
}