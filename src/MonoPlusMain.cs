using MonoPlus.LogSystem;
using Microsoft.Xna.Framework;
using MonoPlus.GraphicsSystem;
using MonoPlus.InputSystem;
using MonoPlus.LocalizationSystem;

namespace MonoPlus;

/// <summary>
/// Wrapper around various <see cref="MonoPlus"/> modules.
/// </summary>
public static class MonoPlusMain
{
    /// <summary>
    /// Whether the program should do operations related to hot reloading assets/.dlls etc. at cost of performance and memory. This does not include <b>fully</b> reloading mods.
    /// </summary>
    public static bool HotReload = true;

    /// <summary>
    /// Call as early as possible. Initializes <see cref="LogHelper"/> and <see cref="OS"/>.
    /// </summary>
    public static void EarlyInitialize()
    {
        OS.Initialize();
        LogHelper.Initialize();
    }

    /// <summary>
    /// Call in <see cref="Game"/>'s constructor.
    /// </summary>
    /// <param name="game">Created game.</param>
    public static void OnGameCreated(Game game)
    {
        Renderer.OnGameCreated(game);
        Locale.FullLoad();
    }

    /// <summary>
    /// Call in <see cref="Game.Initialize"/>.
    /// </summary>
    /// <param name="game">Initializing game.</param>
    public static void OnGameInitialize(Game game)
    {
        Renderer.Initialize(game);
        Input.Initialize(game);
    }
}
