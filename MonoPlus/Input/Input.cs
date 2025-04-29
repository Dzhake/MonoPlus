using System.Linq;
using System.Text;
using CardGames.Console;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoPlus.InputHandling;

public static class Input
{
    /// <summary>
    /// <see cref="StringBuilder"/> which contains "text input" keys pressed since last update, excluding <see cref="char.IsControl(char)"/> keys/>
    /// </summary>
    public static StringBuilder? KeyString;

    /// <summary>
    /// <see cref="KeyboardState"/> for this frame.
    /// </summary>
    public static KeyboardState State;

    /// <summary>
    /// <see cref="TextField"/> which is considered "active".
    /// </summary>
    public static TextField? FocusedTextField;

    /// <summary>
    /// Array of keys which were down last frame
    /// </summary>
    private static Keys[]? PrevPressedKeys;

    /// <summary>
    /// Array of keys which are down this frame
    /// </summary>
    private static Keys[]? PressedKeys;

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
    }

    /// <summary>
    /// Call in <see cref="Game"/>'s <see cref="Game.Update"/>, before anything else.
    /// </summary>
    public static void Update()
    {
        State = Keyboard.GetState();
        PrevPressedKeys = PressedKeys;
        PressedKeys = State.GetPressedKeys();
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

    /// <summary>
    /// Is "L.Shift" or "R.Shift" key is down this frame. Same as Input.Down(Keys.LeftShift) || Input.Down(Keys.RightShift)
    /// </summary>
    public static bool Shift;

    /// <summary>
    /// Is "L.Ctrl" or "R.Ctrl" key is down this frame. Same as Input.Down(Keys.LeftControl) || Input.Down(Keys.RightControl)
    /// </summary>
    public static bool Ctrl;

    /// <summary>
    /// Is "L.Alt" or "R.Alt" key is down this frame. Same as Input.Down(Keys.LeftAlt) || Input.Down(Keys.RightAlt)
    /// </summary>
    public static bool Alt;

    #endregion
}