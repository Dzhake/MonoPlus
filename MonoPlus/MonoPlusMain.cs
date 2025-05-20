using Microsoft.Xna.Framework;
using MonoPlus.Graphics;
using MonoPlus.InputHandling;
using MonoPlus.Logging;

namespace MonoPlus;

/// <summary>
/// Wrapper around various <see cref="MonoPlus"/> modules.
/// </summary>
public static class MonoPlusMain
{
    /// <summary>
    /// Call as early as possible. Initializes <see cref="LoggingHelper"/> and <see cref="OS"/>.
    /// </summary>
    public static void EarlyInitialize()
    {
        OS.Initialize();
        LoggingHelper.Initialize();
    }

    /// <summary>
    /// Call in <see cref="Game"/>'s constructor.
    /// </summary>
    /// <param name="game">Created game.</param>
    public static void OnGameCreated(Game game)
    {
        Renderer.OnGameCreated(game);
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
