using System;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Microsoft.Xna.Framework.Input;
using MonoPlus.Input;
using MonoPlus.Time;
using SDL3;

namespace CardGames.Console;

public class TextField
{
    public StringBuilder text = new();
    public int CursorPos;
    public int SelectionStart;
    public int SelectionEnd;
    public int SelectionLength => SelectionEnd - SelectionStart;

    protected float holdTime;
    protected float totalHoldTime;
    protected Keys holdKey;

    public void Update()
    {
        if (Input.FocusedTextField != this || HandleInput() || Input.KeyString is null) return;
        text.Insert(CursorPos, Input.KeyString);
        CursorPos += Input.KeyString.Length;
    }

    protected bool HandleInput()
    {
        if (KeyPressed(Keys.Back))
        {
            if (!TryRemoveSelection() && CursorPos > 0) RemoveSymbol(CursorPos - 1, true);
        }
        else if (KeyPressed(Keys.Delete))
        {
            if (!TryRemoveSelection() && CursorPos < text.Length) RemoveSymbol(CursorPos);
        }

        else if (KeyPressed(Keys.Left))
            MoveCursor(CursorPos - 1, true);
        else if (KeyPressed(Keys.Right))
            MoveCursor(CursorPos + 1, true);

        else if (KeyPressed(Keys.Home))
            MoveCursor(0);
        else if (KeyPressed(Keys.End))
            MoveCursor(text.Length);
        else if (Input.Ctrl)
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
                string clipboard = SDL.SDL_GetClipboardText();
                text.Insert(CursorPos, clipboard);
                CursorPos += clipboard.Length;
            }
            return true;
        }
        else
            return false;
            
        return true;
    }

    protected bool KeyPressed(Keys key)
    {
        if (Input.Pressed(key))
        {
            ResetHoldInfo();
            holdKey = key;
            return true;
        }
        if (holdKey != key) return false;
        if (Input.Down(holdKey))
        {
            holdTime += Time.DeltaTime;
            totalHoldTime += Time.DeltaTime;
            if (totalHoldTime < 0.5f || holdTime <= 0.05f) return false;
            holdTime = 0;
            return true;
        }
        ResetHoldInfo();
        return false;
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

    protected void MoveCursor(int to, bool affectedBySelection = false)
    {
        if (Input.Shift)
            MoveSelection(to - CursorPos);
        else if (SelectionLength > 0)
        {
            if (affectedBySelection)
            {
                if (to <= CursorPos) CursorPos = SelectionStart;
                else CursorPos = SelectionEnd;
            }
            ResetSelection();
            if (affectedBySelection) return;
        }
            
        CursorPos = Math.Clamp(to, 0, text.Length);
    }

    protected void ResetHoldInfo()
    {
        holdTime = 0;
        totalHoldTime = 0;
        holdKey = Keys.None;
    }

    protected void ResetSelection()
    {
        SelectionStart = 0;
        SelectionEnd = 0;
    }



    public void Reset()
    {
        ResetSelection();
        ResetHoldInfo();
        CursorPos = 0;
        text.Clear();
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
}
