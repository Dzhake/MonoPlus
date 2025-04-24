using Chasm.SemanticVersioning;
using System;

namespace MonoPlus.Modding;

/// <summary>
/// Used to validate and zip <see cref="Mod"/>s
/// </summary>
public static class ModValidator
{
    /// <summary>
    /// Validates and zips <see cref="mod"/>
    /// </summary>
    /// <param name="mod">Mod to build</param>
    /// <returns><see langword="null"/> if build was successfull, or <see cref="Exception"/> if not</returns>
    public static Exception? BuildMod(Mod mod)
    {
        return ValidateMod(mod) ?? ZipMod(mod);
    }

    /// <summary>
    /// Validates <see cref="mod"/>
    /// </summary>
    /// <param name="mod">Mod to validate</param>
    /// <returns><see langword="null"/> if mod is valid, or <see cref="Exception"/> if not</returns>
    public static Exception? ValidateMod(Mod mod)
    {
        ModConfig config = mod.Config;
        ModId id = config.Id;
        string name = id.Name;
        if (id.Version == new SemanticVersion(0, 0, 0))
            return new Exception("Mod version is 0.0.0, you must use 0.0.1 or above");

        if (name is "ExampleMod" or "NewMod")
            return new Exception($"Mod is called {id.Name}, call your mod something original to prevent conflicts with other mods");

        return null;
    }

    /// <summary>
    /// Zips <see cref="mod"/>
    /// </summary>
    /// <param name="mod">Mod to zip</param>
    /// <returns><see langword="null"/> if zipping was successfull, or <see cref="Exception"/> if not</returns>
    public static Exception? ZipMod(Mod mod)
    {
        throw new NotImplementedException("Mod zipping is not yet implemented");
        return null;
    }
}
