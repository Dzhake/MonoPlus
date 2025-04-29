using Microsoft.Xna.Framework;
using System;

namespace MonoPlus.Time;

public static class Time
{
    /// <summary>
    /// Time since last update, in seconds.
    /// </summary>
    public static float UnscaledDeltaTime;
    /// <summary>
    /// Time since program started, in seconds
    /// </summary>
    public static float UnscaledTotalTime;
    /// <summary>
    /// Time since last update, multiplied by time scale multipliers, in seconds.
    /// </summary>
    public static float DeltaTime;
    /// <summary>
    /// Time since program started, multiplied by time scale multipliers at moment those were active, in seconds.
    /// </summary>
    public static float TotalTime;
    /// <summary>
    /// Current time scale.
    /// </summary>
    public static float TimeScale = 1;

    public static event Action TimeSpeedCallback = delegate { };

    public static void Update(GameTime gameTime)
    {
        UnscaledDeltaTime = ((float)gameTime.ElapsedGameTime.TotalMilliseconds) / 1000f;
        UnscaledTotalTime += UnscaledDeltaTime;
        RunTimeScaleCallbacks();
        DeltaTime = UnscaledDeltaTime * TimeScale;
        TotalTime += DeltaTime;
    }

    public static void RunTimeScaleCallbacks()
    {
        TimeScale = 1;
        TimeSpeedCallback.Invoke();
    }
}
