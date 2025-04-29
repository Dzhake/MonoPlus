using Microsoft.Xna.Framework;
using MonoPlus.Graphics;
using MonoPlus.InputHandling;
using MonoPlus.Logging;

namespace MonoPlus;

public static class MonoPlusMain
{
    public static void EarlyInitialize()
    {
        OS.Initialize();
        LoggingHelper.Initialize();
    }

    public static void OnGameCreated(Game game)
    {
        Renderer.OnGameCreated(game);
    }

    public static void OnGameInitialize(Game game)
    {
        Renderer.Initialize(game);
        Input.Initialize(game);
    }
}
