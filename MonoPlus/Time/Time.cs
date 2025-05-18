using Microsoft.Xna.Framework;
using System;
using MonoPlus.Graphics;

namespace MonoPlus.Time;

/// <summary>
/// Class for managing <see cref="DeltaTime"/> and <see cref="TotalTime"/>, and quick access to those.
/// </summary>
public static class Time
{
    /// <summary>
    /// Time since last update.
    /// </summary>
    public static TimeSpan UnscaledDeltaTime;

    /// <summary>
    /// Time since last update, multiplied by <see cref="TimeScale"/>
    /// </summary>
    public static TimeSpan DeltaTimeSpan;

    /// <summary>
    /// Time since last update, multiplied by time scale multipliers, in seconds.
    /// </summary>
    public static float DeltaTime;
    
    /// <summary>
    /// Time since program started.
    /// </summary>
    public static TimeSpan UnscaledTotalTime;

    /// <summary>
    /// Time since program started, multiplied by time scale multipliers at moment those were active.
    /// </summary>
    public static TimeSpan TotalTime;

    /// <summary>
    /// Current time scale.
    /// </summary>
    public static float TimeScale = 1;
    
    /// <summary>
    /// Callback which is called when <see cref="RunTimeScaleCallbacks"/> is called. Subscribe to it if you want to change <see cref="TimeScale"/>.
    /// </summary>
    public static event Action TimeScaleCallback = delegate { };

    /// <summary>
    /// Was the game window active preivous frame.
    /// </summary>
    private static bool wasActive = true;

    /// <summary>
    /// Updates everything related to time: deltaTime, totalTime, timeScale.
    /// </summary>
    /// <param name="gameTime"><see cref="GameTime"/> from your <see cref="Game.Update"/>.</param>
    /// <param name="isActive"></param>
    public static void Update(GameTime gameTime, bool isActive)
    {
        if (!GraphicsSettings.PauseOnFocusLoss) isActive = true;

        if (wasActive)
            UnscaledDeltaTime = gameTime.ElapsedGameTime;
        else
            UnscaledDeltaTime += gameTime.ElapsedGameTime;
        wasActive = isActive;

        if (!isActive) return;
        UnscaledTotalTime += UnscaledDeltaTime;
        RunTimeScaleCallbacks();
        UpdateDeltaTime();
        TotalTime += TimeSpan.FromSeconds(DeltaTime);
    }

    /// <summary>
    /// Resets <see cref="TimeScale"/> to 1 and runs <see cref="TimeScaleCallback"/>.
    /// </summary>
    public static void RunTimeScaleCallbacks()
    {
        TimeScale = 1;
        TimeScaleCallback.Invoke();
    }

    /// <summary>
    /// Updates <see cref="DeltaTimeSpan"/> and <see cref="DeltaTime"/> to match <see cref="UnscaledDeltaTime"/>
    /// </summary>
    public static void UpdateDeltaTime()
    {
        DeltaTimeSpan = UnscaledDeltaTime * TimeScale;
        DeltaTime = (float)DeltaTimeSpan.TotalMicroseconds / 1000000f; //maybe convert after multiplication instead? TODO: research.
    }
}
