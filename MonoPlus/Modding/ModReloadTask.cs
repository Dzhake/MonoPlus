using System.Threading.Tasks;

namespace MonoPlus.Modding;

/// <summary>
/// Represents class which stores <see cref="Task"/> which reloads the mod, and <see cref="Config"/> for that mod.
/// </summary>
/// <param name="task">Task which reloads the mod.</param>
/// <param name="config">Config for that mod.</param>
public struct ModReloadTask(Task task, ModConfig config)
{
    /// <summary>
    /// Task which reloads the mod.
    /// </summary>
    public Task Task = task;

    /// <summary>
    /// Config for that mod.
    /// </summary>
    public ModConfig Config = config;
}
