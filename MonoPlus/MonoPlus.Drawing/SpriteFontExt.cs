using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoPlus.Graphics;

public static class SpriteFontExt
{
    //https://stackoverflow.com/a/34473837
    public static string WrapText(this SpriteFont font, string text, float maxLineWidth)
    {
        string[] words = text.Split(' ');
        StringBuilder sb = new();
        float lineWidth = 0f;
        float spaceWidth = font.MeasureString(" ").X;

        foreach (string word in words)
        {
            Vector2 size = font.MeasureString(word);

            if (lineWidth + size.X < maxLineWidth)
            {
                sb.Append(word + " ");
                lineWidth += size.X + spaceWidth;
            }
            else
            {
                if (size.X > maxLineWidth)
                {
                    if (sb.ToString() == "")
                        sb.Append(font.WrapText(word.Insert(word.Length / 2, " ") + " ", maxLineWidth));
                    else
                        sb.Append("\n" + font.WrapText(word.Insert(word.Length / 2, " ") + " ", maxLineWidth));
                }
                else
                {
                    sb.Append("\n" + word + " ");
                    lineWidth = size.X + spaceWidth;
                }
            }
        }

        return sb.ToString();
    }
}
