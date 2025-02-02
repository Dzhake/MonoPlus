using System.Globalization;
using Microsoft.Xna.Framework;

namespace MonoPlus.Graphics;

public class GraphicUtils
{
    public static Color ParseColor(string hex)
    {
        hex = hex.TrimStart('#');
        return hex.Length >= 6 ? new Color(ParseHex(hex.Substring(0,2)), ParseHex(hex.Substring(2,2)), ParseHex(hex.Substring(4,2)), hex.Length == 8 ? ParseHex(hex.Substring(6,2)) : 255) : Color.White;
    }

    private static int ParseHex(string hex)
    {
        return int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
    }
}
