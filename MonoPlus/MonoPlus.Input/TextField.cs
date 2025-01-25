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
    public int SelectionLength => SelectionEnd - SelectionStart;


    public void Update()
    {
        if (Input.FocusedTextField != this || HandleInput() || Input.KeyString is null) return;
        text.Insert(CursorPos, Input.KeyString);
        CursorPos += Input.KeyString.Length;
    }

    protected bool HandleInput()
    {
        if (Input.Pressed(Keys.Back))
        {
            if (!TryRemoveSelection() && CursorPos > 0) RemoveSymbol(CursorPos - 1, true);
        }
        else if (Input.Pressed(Keys.Delete))
        {
            if (!TryRemoveSelection() && CursorPos < text.Length) RemoveSymbol(CursorPos);
        }

        else if (Input.Pressed(Keys.Left))
            MoveCursor(CursorPos - 1);
        else if (Input.Pressed(Keys.Right))
            MoveCursor(CursorPos + 1);

        else if (Input.Pressed(Keys.End))
            MoveCursor(text.Length);
        else if (Input.Pressed(Keys.Home))
            MoveCursor(0);
        /*else if (Input.Ctrl)
        {
            bool cut = Input.Pressed(Keys.X);
            if ((Input.Pressed(Keys.C) || cut) && SelectionLength > 0)
            {
                SDL.SDL_SetClipboardText(GetSelection());
                if (cut) RemoveSelection();
            }
            else if (Input.Pressed(Keys.V))
            {
                TryRemoveSelection();
                text.Insert(CursorPos, SDL.SDL_GetClipboardText());
            }
            return true;
        }*/
        else
            return false;
        return true;
    }


    public bool TryRemoveSelection()
    {
        if (SelectionStart == SelectionEnd) return false;
        RemoveSelection();
        return true;
    }

    public void RemoveSelection()
    {
        if (CursorPos == SelectionEnd) CursorPos = SelectionStart;
        text.Remove(SelectionStart, SelectionLength);
        ResetSelection();
    }

    public string GetSelection()
    {
        return text.ToString().Substring(SelectionStart, SelectionLength);
    }


    protected void RemoveSymbol(int index, bool moveCursor = false)
    {
        text.Remove(index, 1);
        if (moveCursor) CursorPos = Math.Clamp(CursorPos - 1, 0, text.Length);
    }

    /// <summary>
    /// Moves bound of selection which is at same position as <see cref="CursorPos"/>. Call BEFORE changing <see cref="CursorPos"/>!
    /// </summary>
    /// <param name="amount">Amount to move</param>
    protected void MoveSelection(int amount)
    {
        if (SelectionStart == SelectionEnd)
        {
            switch (amount)
            {
                case 0:
                    return;
                case < 0:
                    SelectionEnd = CursorPos;
                    SelectionStart = CursorPos + amount;
                    break;
                case > 0:
                    SelectionStart = CursorPos;
                    SelectionEnd = CursorPos + amount;
                    break;
            }
            SelectionStart = Math.Clamp(SelectionStart, 0, text.Length);
            SelectionEnd = Math.Clamp(SelectionEnd, 0, text.Length);
            return;
        }

        if (CursorPos == SelectionStart)
            SelectionStart = Math.Clamp(SelectionStart + amount, 0, text.Length);
        else if (CursorPos == SelectionEnd)
            SelectionEnd = Math.Clamp(SelectionEnd + amount, 0, text.Length);
        else
            ResetSelection();

        if (SelectionStart == SelectionEnd) ResetSelection();

        if (SelectionEnd < SelectionStart)
            (SelectionEnd, SelectionStart) = (SelectionStart, SelectionEnd);
    }

    protected void ResetSelection()
    {
        SelectionStart = 0;
        SelectionEnd = 0;
    }

    protected void MoveCursor(int to)
    {
        if (Input.Shift)
            MoveSelection(to - CursorPos);
        else if (SelectionLength > 0)
        {
            ResetSelection();
            return;
        }
            
        CursorPos = Math.Clamp(to, 0, text.Length);
    }
}
