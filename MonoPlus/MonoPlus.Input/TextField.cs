using System;
using System.Text;
using Microsoft.Xna.Framework.Input;
using MonoPlus.Input;

namespace CardGames.Console;

public class TextField
{
    public StringBuilder text = new();
    public int CursorPos;
    public int SelectionStart;
    public int SelectionEnd;

    public void Update()
    {
        if (Input.FocusedTextField != this) return; 
        
        text.Append(Input.KeyString);
        if (Input.Pressed(Keys.Back) && CursorPos > 0)
            text.Remove(CursorPos - 1, 1);
        if (Input.Pressed(Keys.Delete) && CursorPos < text.Length)
            text.Remove(CursorPos, 1);
        if (Input.Pressed(Keys.Left))
        {
            CursorPos = Math.Clamp(CursorPos - 1, 0, text.Length);
            if (!Input.Shift)
                ResetSelection();
        }

        if (Input.Pressed(Keys.Right))
        {
            CursorPos = Math.Clamp(CursorPos + 1, 0, text.Length);
            if (!Input.Shift)
                ResetSelection();
        }

        if (Input.Pressed(Keys.End))
            CursorPos = text.Length;
        if (Input.Pressed(Keys.Home))
            CursorPos = 0;
    }

    private void ResetSelection()
    {
        SelectionStart = 0;
        SelectionEnd = 0;
    }
}
