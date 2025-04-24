using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Chasm.SemanticVersioning;

namespace MonoPlus.Modding;

/// <summary>
/// Used to sort <see cref="ModConfig"/>s, into <see cref="List{ModConfig}"/> which be safely loaded with <see langword="foreach"/>, because <see cref="Mod"/>s which depend on others <see cref="Mod"/>s are after dependencies in the sorted list. Uses Tarjan's strongly connected components algorithm.
/// </summary>
public class ModConfigSorter
{
    /// <summary>
    /// Represents state of <see cref="ModConfig"/> for <see cref="ModConfigSorter.SortModConfigs"/>
    /// </summary>
    public enum ModSortState
    {
        /// <summary>
        /// Mod was not yet visited by sorting algorithm
        /// </summary>
        NotVisited,
        /// <summary>
        /// Mod was visited, but algorithm didn't finish with it, so algorithm is either looking at it or some child node
        /// </summary>
        Visiting,
        /// <summary>
        /// Mod was visited successfully
        /// </summary>
        Visited
    }

    /// <summary>
    /// Sorts <see cref="ModConfig"/>s, into <see cref="List{ModConfig}"/> which be safely loaded with <see langword="foreach"/>, because <see cref="Mod"/>s which depend on others <see cref="Mod"/>s are after dependencies in the sorted list.
    /// </summary>
    /// <param name="configs"><see cref="List{T}"/> of <see cref="ModConfig"/>s to sort</param>
    [Pure]
    public static List<ModConfig> SortModConfigs(List<ModConfig> configs)
    {
        List<ModConfig> sorted = new();

        //All are NotVisited by default
        ModSortState[] sortStates = new ModSortState[configs.Count];

        for (var i = 0; i < configs.Count; i++)
            SortStep(i, sorted, sortStates, configs);

        return sorted;
    }


    private static void SortStep(int index, List<ModConfig> sorted, ModSortState[] sortStates, List<ModConfig> configs)
    {
        switch (sortStates[index])
        {
        case ModSortState.NotVisited:
            //If not visited, then visit all dependencies
            var config = configs[index];
            sortStates[index] = ModSortState.Visiting;

            //if dependencies are null then just instantly mark as visited
            if (config.Dependencies is not null)
                foreach (ModDep dep in config.Dependencies)
                {
                    //check for mods from ModManager.Mods
                    if (ModLoader.loadedAnyMods && ModManager.Mods.Values.Any(mod => mod.Config.Id.Matches(dep))) continue;
                    int depIndex = configs.FindIndex(otherConfig => otherConfig.Id.Matches(dep));
                    if (depIndex == -1) throw new ModDependencyNotFoundException(config.Id, dep);
                    SortStep(depIndex, sorted, sortStates, configs);
                }
            sortStates[index] = ModSortState.Visited;
            sorted.Add(config);
            return;
        case ModSortState.Visiting:
            //cyclic dependency
            string thisId = configs[index].Id.Name;
            throw new CyclicModDependencyException(configs[index].Id, configs.Find(otherConfig => otherConfig.Dependencies?.Any(dep => dep.Name == thisId) ?? false)?.Id ?? new ModId("TRIED TO FIND CYCLIC MOD DEPENDENCY, BUT IT WAS NOT FOUND???", new SemanticVersion(0, 0, 0)));
        case ModSortState.Visited:
            //if visited, then return
            return;
        default:
            throw new InvalidOperationException("ModSortState is not any known state!");
        }
    }
}
