using System.Collections.Generic;
using System.Linq;
using System.Text;
using CardGames.Console;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoPlus.Input;

public static class Input
{
    private static Keys[]? PrevPressedKeys;
    private static Keys[]? PressedKeys;
    public static StringBuilder? KeyString;
    public static KeyboardState State;
    public static TextField? FocusedTextField;
    public static bool DoModifierChecks = true;

    private static void TextInput(object? sender, TextInputEventArgs e)
    {
        if (char.IsControl(e.Character)) return;
        KeyString?.Append(e.Character);
    }

    #region RequiredToCall
    /// <summary>
    /// Call in <see cref="Game"/>'s <see cref="Game.Initialize"/>.
    /// </summary>
    /// <param name="game">Current game.</param>
    public static void Initialize(Game game)
    {
        KeyString = new StringBuilder();
        GameWindow win = game.Window;
        win.TextInput += TextInput;
        DoModifierChecks = true;
    }

    /// <summary>
    /// Call in <see cref="Game"/>'s <see cref="Game.Update"/>, before anything else.
    /// </summary>
    public static void Update()
    {
        State = Keyboard.GetState();
        PrevPressedKeys = PressedKeys;
        PressedKeys = State.GetPressedKeys();
        if (!DoModifierChecks) return;
        Shift = Down(Keys.LeftShift) || Down(Keys.RightShift);
        Ctrl = Down(Keys.LeftControl) || Down(Keys.RightControl);
        Alt = Down(Keys.LeftAlt) || Down(Keys.RightAlt);
    }

    /// <summary>
    /// Call in <see cref="Game.Update"/>, after everything else.
    /// </summary>
    public static void PostUpdate()
    {
        KeyString?.Clear();
    }
    #endregion

    #region PublicChecks
    /// <summary>
    /// Returns true if key is down this frame, and wasn't down previous frame.
    /// </summary>
    /// <param name="key">Key to check</param>
    /// <returns>Whether the key was pressed</returns>
    public static bool Pressed(Keys key) => PrevPressedKeys != null && PressedKeys != null && !PrevPressedKeys.Contains(key) && PressedKeys.Contains(key);

    /// <summary>
    /// Returns true if key is down this frame.
    /// </summary>
    /// <param name="key">Key to check</param>
    /// <returns>Whether the key is down</returns>
    public static bool Down(Keys key) => PressedKeys != null && PressedKeys.Contains(key);

    /// <summary>
    /// Returns true if key is down this frame, and was down previous frame.
    /// </summary>
    /// <param name="key">Key to check</param>
    /// <returns>Whether the key is down and not pressed</returns>
    public static bool DownOnly(Keys key) => PrevPressedKeys != null && PressedKeys != null && PrevPressedKeys.Contains(key) && PressedKeys.Contains(key);

    /// <summary>
    /// Returns true if key isn't down this frame, and was down previous frame.
    /// </summary>
    /// <param name="key">Key to check</param>
    /// <returns>Whether the key is released</returns>
    public static bool Released(Keys key) => PrevPressedKeys != null && PressedKeys != null && PrevPressedKeys.Contains(key) && !PressedKeys.Contains(key);

    public static bool Shift = false;
    public static bool Ctrl = false;
    public static bool Alt = false;

    #endregion
}