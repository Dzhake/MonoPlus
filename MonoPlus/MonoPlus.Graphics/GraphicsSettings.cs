using Microsoft.Xna.Framework;
using SDL3;

namespace MonoPlus.Graphics;

/// <summary>
///     Contains graphics info which should be serialized in settings file and some methods to change those.
/// </summary>
public static class GraphicsSettings
{
    /// <summary>
    /// List of common 16:9 window sizes/resolutions
    /// </summary>
    public static Vector2[] CommonResolutions16x9 =
    {
        new (640, 360), new(1280, 720), new(1600, 900), new(1920, 1080), new(2560, 1440), new(3840,2160)
    };

    /// <summary>
    /// List of common 4:3 window sizes/resolutions
    /// </summary>
    public static Vector2[] CommonResolutions4x3 = { new (640, 480), new(800, 600), new(1600, 1200) };


    public static Vector2 WindowSize = new(1280, 720);
    public static bool PauseOnFocusLoss;

    /// <summary>
    /// Changes window behaviour to fullscreen or windowed.
    /// </summary>
    public static void ApplyFullscreenChanges(bool fullscreen)
    {
        GraphicsDeviceManager? deviceManager = Renderer.deviceManager;
        if (deviceManager is null || Renderer.Window is null) return;

        switch (fullscreen)
        {
            case true:
                Rectangle windowBounds = Renderer.Window.ClientBounds;
                deviceManager.PreferredBackBufferWidth = windowBounds.Width;
                deviceManager.PreferredBackBufferHeight = windowBounds.Height;
                break;
            case false:
                SDL.SDL_RestoreWindow(Renderer.WindowHandle);
                WindowSize = new(1280, 720);
                ApplyWindowSizeChanges();
                break;
        }

        deviceManager.IsFullScreen = fullscreen;
        deviceManager.HardwareModeSwitch = !fullscreen;
        deviceManager.ApplyChanges();
    }

    /// <summary>
    /// Changes PreferredBackBuffer sizes and centers game's window in windowed mode
    /// </summary>
    public static unsafe void ApplyWindowSizeChanges()
    {
        if (Renderer.deviceManager is null || Renderer.Window is null) return;
        Renderer.deviceManager.PreferredBackBufferWidth = (int)WindowSize.X;
        Renderer.deviceManager.PreferredBackBufferHeight = (int)WindowSize.Y;

        SDL.SDL_DisplayMode* displayMode = SDL.SDL_GetCurrentDisplayMode(0);
        Vector2 windowPos = (new Vector2(displayMode->w, displayMode->h) - WindowSize) / 2;
        Renderer.Window.Position = windowPos.ToPoint();

        Renderer.deviceManager.ApplyChanges();
    }
}