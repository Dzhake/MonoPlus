using System;
using System.Text;
using Microsoft.Xna.Framework.Input;
using MonoPlus.InputHandling;
using MonoPlus.Time;
using SDL3;

namespace CardGames.Console;

/// <summary>
/// Represents text input field which captures input and may have selection.
/// </summary>
public class TextField
{
    /// <summary>
    /// Current field input.
    /// </summary>
    public StringBuilder text = new();

    /// <summary>
    /// Cursor's position in field.
    /// </summary>
    public int CursorPos;

    /// <summary>
    /// Position of selection's start.
    /// </summary>
    public int SelectionStart;

    /// <summary>
    /// Position of selection's end.
    /// </summary>
    public int SelectionEnd;

    /// <summary>
    /// Total length of selection.
    /// </summary>
    public int SelectionLength => SelectionEnd - SelectionStart;

    /// <summary>
    /// Time since last fake <see cref="heldKey"/> press.
    /// </summary>
    protected float heldTime;

    /// <summary>
    /// For how long <see cref="heldKey"/> is being held.
    /// </summary>
    protected float totalHeldTime;

    /// <summary>
    /// Key which user holds.
    /// </summary>
    protected Keys heldKey;

    /// <summary>
    /// Updates field if <see cref="Input.FocusedTextField"/> is <see langword="this"/>
    /// </summary>
    public void Update()
    {
        if (Input.FocusedTextField != this || HandleInput() || Input.KeyString is null) return;
        text.Insert(CursorPos, Input.KeyString);
        CursorPos += Input.KeyString.Length;
    }

    /// <summary>
    /// Handles keybinds input.
    /// </summary>
    /// <returns>If any input was used.</returns>
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
            else if (Input.Pressed(Keys.A))
            {
                SelectionEnd = text.Length;
                CursorPos = SelectionEnd;
            }
        }
        else
            return false;
            
        return true;
    }

    /// <summary>
    /// Check if key is pressed, or is being held and it's time for fake press, so holding a key repeats like it's being pressed.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <returns>Whether the key is pressed.</returns>
    protected bool KeyPressed(Keys key)
    {
        if (Input.Pressed(key))
        {
            ResetHoldInfo();
            heldKey = key;
            return true;
        }
        if (heldKey != key) return false;
        if (Input.Down(heldKey))
        {
            heldTime += Time.DeltaTime;
            totalHeldTime += Time.DeltaTime;
            if (totalHeldTime < 0.5f || heldTime <= 0.05f) return false;
            heldTime = 0;
            return true;
        }
        ResetHoldInfo();
        return false;
    }

    /// <summary>
    /// Removes symbol in <see cref="text"/> at specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">Index of the symbol in <see cref="text"/>.</param>
    /// <param name="moveCursor">Should move cursor to match new string.</param>
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

    /// <summary>
    /// Move cursor to specified position.
    /// </summary>
    /// <param name="to">New cursor's position.</param>
    /// <param name="affectedBySelection">Whether the cursor should stick to selection's boundaries if selection exists and <see cref="Input.Shift"/> is not down.</param>
    protected void MoveCursor(int to, bool affectedBySelection = false)
    {
        if (Input.Shift)
            MoveSelection(to - CursorPos);
        else if (SelectionLength > 0)
        {
            if (affectedBySelection)
            {
                CursorPos = to <= CursorPos ? SelectionStart : SelectionEnd;
            }
            ResetSelection();
            if (affectedBySelection) return;
        }
            
        CursorPos = Math.Clamp(to, 0, text.Length);
    }

    /// <summary>
    /// Reset all info about <see cref="heldKey"/>.
    /// </summary>
    protected void ResetHoldInfo()
    {
        heldTime = 0;
        totalHeldTime = 0;
        heldKey = Keys.None;
    }

    /// <summary>
    /// Reset selection to non-existing.
    /// </summary>
    protected void ResetSelection()
    {
        SelectionStart = 0;
        SelectionEnd = 0;
    }

    /// <summary>
    /// Reset entire field to default state.
    /// </summary>
    public void Reset()
    {
        ResetSelection();
        ResetHoldInfo();
        CursorPos = 0;
        text.Clear();
    }

    /// <summary>
    /// Try to remove all symbols from <see cref="SelectionStart"/> to <see cref="SelectionEnd"/>.
    /// </summary>
    /// <returns>Whether the selection was not empty.</returns>
    public bool TryRemoveSelection()
    {
        if (SelectionStart == SelectionEnd) return false;
        RemoveSelection();
        return true;
    }

    /// <summary>
    /// Remove all symbols from <see cref="SelectionStart"/> to <see cref="SelectionEnd"/>.
    /// </summary>
    public void RemoveSelection()
    {
        if (CursorPos == SelectionEnd) CursorPos = SelectionStart;
        text.Remove(SelectionStart, SelectionLength);
        ResetSelection();
    }

    /// <summary>
    /// Get currently selected text.
    /// </summary>
    /// <returns>Currently selected text.</returns>
    public string GetSelection()
    {
        return text.ToString().Substring(SelectionStart, SelectionLength);
    }
}
