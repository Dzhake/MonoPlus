using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoPlus.GraphicsSystem.BitmapFonts;

/// <summary>
/// Represents a font, which contains a <see cref="Texture"/> with all the glyphs in a row of same <see cref="GlyphSize"/>, and a definition of <see cref="Glyphs"/> to match where in <see cref="Texture"/> is which glyph.
/// </summary>
public class BitmapFont
{
    /// <summary>
    /// Represents serializable information about <see cref="BitmapFont"/>. <see cref="Texture"/> is not included.
    /// </summary>
    public struct Info
    {
        /// <summary>
        /// Size of each glyph in <see cref="Texture"/>.
        /// </summary>
        [JsonInclude]
        public Point GlyphSize;

        /// <summary>
        /// Amount of empty pixels between letters. X spacing is between letters in same line, Y spacing is between lines.
        /// </summary>
        [JsonInclude] public Point Spacing;
        
        /// <summary>
        /// List of glyphs in <see cref="Texture"/>.
        /// </summary>
        [JsonInclude]
        public string Glyphs;
    }

    /// <summary>
    /// Texture with all the glyphs in a row of same <see cref="GlyphSize"/>.
    /// </summary>
    public readonly Texture2D Texture;

    /// <summary>
    /// Size of each glyph in <see cref="Texture"/>.
    /// </summary>
    public Point GlyphSize;

    /// <summary>
    /// Amount of empty pixels between letters. X spacing is between letters in same line, Y spacing is between lines.
    /// </summary>
    public Point Spacing;

    /// <summary>
    /// List of glyphs in <see cref="Texture"/>.
    /// </summary>
    public readonly string Glyphs;

    /// <summary>
    /// Instances a new <see cref="BitmapFont"/> with the specified <see cref="Texture"/>, <see cref="GlyphSize"/> and <see cref="Glyphs"/>.
    /// </summary>
    /// <param name="texture">Texture with all the glyphs in a row of same <see cref="GlyphSize"/>.</param>
    /// <param name="glyphSize">Size of each glyph in <see cref="Texture"/>.</param>
    /// <param name="spacing">Amount of empty pixels between letters. X spacing is between letters in same line, Y spacing is between lines.</param>
    /// <param name="glyphs">List of glyphs in <see cref="Texture"/>.</param>
    public BitmapFont(Texture2D texture, Point glyphSize, Point spacing, string glyphs)
    {
        Texture = texture;
        GlyphSize = glyphSize;
        Spacing = spacing;
        Glyphs = glyphs;
    }

    /// <summary>
    /// Instances a new <see cref="BitmapFont"/> with the specified <see cref="Texture"/> and <paramref name="info"/>.
    /// </summary>
    /// <param name="texture">Texture with all the glyphs in a row of same <see cref="GlyphSize"/>.</param>
    /// <param name="info">Information about bitmap font.</param>
    public BitmapFont(Texture2D texture, Info info) : this(texture, info.GlyphSize, info.Spacing,info.Glyphs) {}

    /// <summary>
    /// Get texture of rendered <paramref name="text"/> for caching. Uses <see cref="Renderer.spriteBatch"/>.
    /// </summary>
    /// <param name="text">Text to render with this <see cref="BitmapFont"/>.</param>
    /// <param name="color">A color mask.</param>
    /// <param name="rotation">A rotation of this sprite.</param>
    /// <param name="origin">Center of the rotation. 0,0 by default.</param>
    /// <param name="scale">A scaling of this sprite.</param>
    /// <param name="effects">Modificators for drawing. Can be combined.</param>
    /// <param name="layerDepth">A depth of the layer of this sprite.</param>
    public Texture2D GetStringTexture(string text, Color? color = null, float rotation = 0f, Vector2? origin = null, Vector2? scale = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
    {
        color ??= Color.White;
        scale ??= Vector2.One;
        origin ??= Vector2.Zero;

        Point stringSize = MeasureString(text);

        RenderTarget2D? previousRenderTarget = Renderer.RenderTarget;
        RenderTarget2D renderTarget = Renderer.CreateRenderTarget(stringSize.X, stringSize.Y);

        Renderer.SetRenderTarget(renderTarget);
        DrawText(text, Renderer.spriteBatch, Vector2.Zero, color, rotation, origin, scale, effects, layerDepth);

        Renderer.SetRenderTarget(previousRenderTarget);

        return renderTarget;
    }

    /// <summary>
    /// Draws <paramref name="text"/> via <paramref name="spriteBatch"/> with specified options using this <see cref="BitmapFont"/>. <paramref name="spriteBatch"/> must be active.
    /// </summary>
    /// <param name="text">Text to draw.</param>
    /// <param name="spriteBatch">Active <see cref="SpriteBatch"/>, which will draw the text.</param>
    /// <param name="position">The drawing location on render target.</param>
    /// <param name="color">A color mask.</param>
    /// <param name="rotation">A rotation of this sprite.</param>
    /// <param name="origin">Center of the rotation. 0,0 by default.</param>
    /// <param name="scale">A scaling of this sprite.</param>
    /// <param name="effects">Modificators for drawing. Can be combined.</param>
    /// <param name="layerDepth">A depth of the layer of this sprite.</param>
    public void DrawText(string text, SpriteBatch spriteBatch, Vector2 position, Color? color = null, float rotation = 0f, Vector2? origin = null, Vector2? scale = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0)
    {
        color ??= Color.White;
        scale ??= Vector2.One;
        origin ??= Vector2.Zero;
        Vector2 currentPos = position;
        foreach (char glyph in text)
        {
            Rectangle sourceRectangle = GetGlyphSourceRectangle(glyph);
            
            if (sourceRectangle.X < 0) //Glyph not found.
            {
                switch (glyph)
                {
                    case '\n':
                        currentPos.Y += (GlyphSize.Y + Spacing.Y) * scale.Value.Y;
                        currentPos.X = position.X;
                        continue;
                    case ' ':
                        sourceRectangle = Rectangle.Empty;
                        break;
                }
            }
            
            spriteBatch.Draw(Texture, currentPos, sourceRectangle, (Color)color, rotation, (Vector2)origin, (Vector2)scale, effects, layerDepth);
            currentPos.X += (GlyphSize.X + Spacing.X) * scale.Value.X;
        }
    }

    /// <summary>
    /// Get size of <paramref name="text"/>, if it would be rendered. Unlike <see cref="MeasureOneLine"/> uses newlines.
    /// </summary>
    /// <param name="text">Text, whose size to check.</param>
    /// <returns>Size of <paramref name="text"/> if it would be rendered.</returns>
    public Point MeasureString(string text)
    {
        string[] lines = text.Split('\n');
        int maxLength = lines[0].Length;
        for (int i = 1; i < lines.Length; i++)
        {
            int length = lines[i].Length;
            if (length > maxLength) maxLength = length;
        }

        return new(MeasureOneLine(maxLength), GlyphSize.Y + lines.Length * (GlyphSize.Y + Spacing.Y));
    }

    /// <summary>
    /// Get size X of text with <paramref name="glyphsAmount"/> chars, if it would be rendered, ignoring newlines.
    /// </summary>
    /// <param name="glyphsAmount">Amount of chars in given text.</param>
    /// <returns>Size X of text with <paramref name="glyphsAmount"/> chars, if it would be rendered.</returns>
    public int MeasureOneLine(int glyphsAmount) => (GlyphSize.X + Spacing.X) * glyphsAmount;

    /// <summary>
    /// Get source rectangle of glyph in <see cref="Texture"/> based on it's char index in <see cref="Glyphs"/>.
    /// </summary>
    /// <param name="index">Index of the glyph's char in <see cref="Glyphs"/>.</param>
    /// <returns>Source rectangle of the glyph.</returns>
    public Rectangle GetGlyphSourceRectangle(int index) => new(GlyphSize.X * index, 0, GlyphSize.X, GlyphSize.Y);

    /// <summary>
    /// Get source rectangle of glyph in <see cref="Texture"/> based in it's char. Uses <see cref="GetGlyphSourceRectangle(int)"/> internally. If glyph is not found in <see cref="Glyphs"/>, X of returned <see cref="Rectangle"/> will be negative.
    /// </summary>
    /// <param name="glyph">Glyph's char.</param>
    /// <returns>Source rectangle of the glyph.</returns>
    public Rectangle GetGlyphSourceRectangle(char glyph) => GetGlyphSourceRectangle(Glyphs.IndexOf(glyph));
}
