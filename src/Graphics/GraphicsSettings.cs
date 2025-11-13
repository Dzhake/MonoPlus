using Microsoft.Xna.Framework;
using SDL3;

namespace Monod.GraphicsSystem;

/// <summary>
/// Represents graphics info which should be serialized in settings file and some methods to change those..
/// </summary>
public static class GraphicsSettings
{
    /// <summary>
    /// Represents types of behaviour how game should act while not active.
    /// </summary>
    public enum OnFocusLossBehaviour
    {
        /// <summary>
        /// Game continues running like it's active.
        /// </summary>
        Continue,

        /// <summary>
        /// Game will not update, but once it's active it'll run like all that time it was active, by storing deltaTime while not active, and once active using it all for first frame.
        /// </summary>
        Eco,

        /// <summary>
        /// Game will not update, and once it's active it'll continue like normal.
        /// </summary>
        TemporaryStop,

        /// <summary>
        /// Game will not update, and once it's active it will be paused.
        /// </summary>
        FullStop,
    }

    /// <summary>
    /// List of common 16:9 window sizes/resolutions.
    /// </summary>
    public static Vector2[] CommonResolutions16x9 =
    {
        new (640, 360), new(1280, 720), new(1600, 900), new(1920, 1080), new(2560, 1440), new(3840,2160)
    };

    /// <summary>
    /// List of common 4:3 window sizes/resolutions.
    /// </summary>
    public static Vector2[] CommonResolutions4x3 = { new (640, 480), new(800, 600), new(1600, 1200) };

    /// <summary>
    /// Game window's size in pixels.
    /// </summary>
    public static Vector2 WindowSize = new(1280, 720);

    /// <summary>
    /// Whether the game should not update while the game is paused.
    /// </summary>
    public static OnFocusLossBehaviour FocusLossBehaviour = OnFocusLossBehaviour.Eco;

    /// <summary>
    /// Changes window behaviour to fullscreen or windowed.
    /// </summary>
    public static void ApplyFullscreenChanges(bool fullscreen)
    {
        GraphicsDeviceManager deviceManager = Renderer.deviceManager;

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
        Renderer.deviceManager.PreferredBackBufferWidth = (int)WindowSize.X;
        Renderer.deviceManager.PreferredBackBufferHeight = (int)WindowSize.Y;

        SDL.SDL_DisplayMode* displayMode = SDL.SDL_GetCurrentDisplayMode(0);
        Vector2 windowPos = (new Vector2(displayMode->w, displayMode->h) - WindowSize) / 2;
        Renderer.Window.Position = windowPos.ToPoint();

        Renderer.deviceManager.ApplyChanges();
    }
}