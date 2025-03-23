using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoPlus.Graphics;

public static class SpriteFontExt
{
    private static void WrapTextBase(this SpriteFont font, string text, float maxLineWidth, Action<string> onWrap, float startOffset = 0)
    {
        string[] words = text.Split(' ');
        StringBuilder sb = new();
        float lineWidth = startOffset;
        float spaceWidth = font.MeasureString(" ").X;

        foreach (string word in words)
        {
            Vector2 size = font.MeasureString(word);

            if (lineWidth + size.X < maxLineWidth)
            {
                sb.Append($"{word} ");
                lineWidth += size.X + spaceWidth;
            }
            else
            {
                onWrap(sb.ToString());
                sb.Clear();
                sb.Append($"{word} ");
                lineWidth = size.X + spaceWidth;
            }
        }
    }

    public static string WrapText(this SpriteFont font, string text, float maxLineWidth, float startOffset = 0)
    {
        StringBuilder sb = new();
        WrapTextBase(font, text, maxLineWidth, s => sb.Append($"{s}\n"), startOffset);
        return sb.ToString();
    }

    public static List<string> WrapTextToList(this SpriteFont font, string text, float maxLineWidth, float startOffset = 0)
    {
        List<string> result = new();
        WrapTextBase(font, text, maxLineWidth, result.Add, startOffset);
        return result;
    }

    public static List<int> WrapTextPositions(this SpriteFont font, string text, float maxLineWidth, out float offsetX, float startOffset = 0)
    {
        offsetX = startOffset;
        List<int> result = new();
        string lastStr = "";
        WrapTextBase(font, text, maxLineWidth, s => {result.Add(s.Length); lastStr = s;}, startOffset);
        offsetX = font.MeasureString(lastStr).X;
        return result;
    }
}
