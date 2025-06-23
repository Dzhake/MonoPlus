
using System;
using HarmonyLib;

namespace MonoPlus.ModSystem;

/// <summary>
/// Small utility class for <see cref="Harmony"/> extensions
/// </summary>
public static class HarmonyExtensions
{
    /// <summary>
    /// Unpatches all patches made by this <see cref="Harmony"/>.
    /// </summary>
    /// <param name="harmony">Harmony whos patches to unpatch.</param>
    public static void UnpatchSelf(this Harmony harmony)
    {
        harmony.UnpatchAll(harmony.Id);
    }

    /// <summary>
    /// Applies all patches from specified <paramref name="type"/>. <paramref name="type"/> must have <see cref="HarmonyPatch"/> attribute.
    /// </summary>
    /// <param name="harmony">Instance of harmony which should apply patches</param>
    /// <param name="type">Type, which has methods with <see cref="HarmonyPatch"/> attributes, and has <see cref="HarmonyPatch"/> attribute.</param>
    public static void PatchType(this Harmony harmony, Type type) => new PatchClassProcessor(harmony, type).Patch();
}
