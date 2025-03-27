using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoPlus.Graphics;

public static class Renderer
{
    public static GraphicsDeviceManager? deviceManager;
    public static SpriteBatch? spriteBatch;
    public static GraphicsDevice? device;
    public static Texture2D Pixel = null!;
    public static IntPtr WindowHandle;
    public static GameWindow? Window;

    public static void OnGameCreated(Game game)
    {
        deviceManager = new GraphicsDeviceManager(game);
    }

    public static void Initialize(Game game)
    {
        if (deviceManager == null) throw new InvalidOperationException("deviceManager is null!");
        device = game.GraphicsDevice;
        Window = game.Window;
        WindowHandle = Window.Handle;
        spriteBatch = new SpriteBatch(game.GraphicsDevice);
        Pixel = new(device, 1, 1);
        Pixel.SetData([Color.White]);
    }

    #region BaseFunctions
    public static void Clear(Color color) => device?.Clear(color);

    public static void DrawTexture(Texture2D texture, Vector2 position, Color color) => spriteBatch?.Draw(texture, position, color);

    public static void DrawTexture(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color,
    float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0) => spriteBatch?.Draw(texture,
    position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);

    public static void DrawText(SpriteFont font, string text, Vector2 position, Color color) => spriteBatch?.DrawString(font, text, position, color);

    public static void DrawText(SpriteFont font, string text, Vector2 position, Color color, float rotation, Vector2? origin = null, Vector2? scale = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0, bool rtl = false) => spriteBatch?.DrawString(font, text, position, color, rotation, origin ?? Vector2.One, scale ?? Vector2.One, effects, layerDepth, rtl);

    public static void Begin(SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState? blendState = null, SamplerState? samplerState = null, DepthStencilState? depthStencilState = null, RasterizerState? rasterizerState = null, Effect? effect = null, Matrix? transformMatrix = null) => spriteBatch?.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);

    public static void End() => spriteBatch?.End();
    #endregion

    #region ShapesFunctions
    public static void DrawLine(Vector2 lineStart, Vector2 lineEnd, Color color, float width = 1f)
    {
        //if (Pixel is null) throw new InvalidOperationException("DrawLine was called, but Pixel is null!");
        DrawTexture(Pixel, lineStart, new Rectangle?(), color, (float)Math.Atan2(lineEnd.Y - lineStart.Y, lineEnd.X - lineStart.X), new Vector2(0f, 0.5f), new Vector2((lineStart - lineEnd).Length(), width));
    }


    /// <summary>
    /// Draws a rectangle.
    /// </summary>
    /// <param name="p1">Rectangle's top left corner</param>
    /// <param name="p2">Rectangle's bottom right corner</param>
    /// <param name="color">Rectangle's color</param>
    public static void DrawRect(Vector2 p1, Vector2 p2, Color color) =>
        DrawTexture(Pixel, p1, new Rectangle?(), color, 0f, Vector2.Zero, new Vector2(-(p1.X - p2.X), -(p1.Y - p2.Y)));

    /// <summary>
    /// Draws a rectangle.
    /// </summary>
    /// <param name="rectangle"><see cref="Rectangle"/> which represents drawn rectangle's position and size.</param>
    /// <param name="color">Rectangle's color</param>
    public static void DrawRect(Rectangle rectangle, Color color) => DrawRect(new Vector2(rectangle.Left, rectangle.Top), new Vector2(rectangle.Right, rectangle.Bottom), color);

    /// <summary>
    /// Draws a rectangle.
    /// </summary>
    /// <param name="p1">Rectangle's top left corner</param>
    /// <param name="p2">Rectangle's bottom right corner</param>
    /// <param name="color">Rectangle's color</param>
    /// <param name="borderWidth">Width of lines used to make rectangle.</param>
    public static void DrawHollowRect(Vector2 p1, Vector2 p2, Color color, float borderWidth = 1f)
    {
        float halfBorderWidth = borderWidth / 2f;
        DrawLine(new Vector2(p1.X, p1.Y + halfBorderWidth), new Vector2(p2.X, p1.Y + halfBorderWidth), color, borderWidth);
        DrawLine(new Vector2(p1.X + halfBorderWidth, p1.Y + borderWidth), new Vector2(p1.X + halfBorderWidth, p2.Y - borderWidth), color, borderWidth);
        DrawLine(new Vector2(p2.X, p2.Y - halfBorderWidth), new Vector2(p1.X, p2.Y - halfBorderWidth), color, borderWidth);
        DrawLine(new Vector2(p2.X - halfBorderWidth, p2.Y - borderWidth), new Vector2(p2.X - halfBorderWidth, p1.Y + borderWidth), color, borderWidth);
    }

    /// <summary>
    /// Draws a rectangle.
    /// </summary>
    /// <param name="rectangle"><see cref="Rectangle"/> which represents drawn rectangle's position and size.</param>
    /// <param name="color">Rectangle's color</param>
    /// <param name="borderWidth">Width of lines used to make rectangle.</param>
    public static void DrawHollowRect(Rectangle rectangle, Color color, float borderWidth = 1f) =>
        DrawHollowRect(new Vector2(rectangle.Left, rectangle.Top), new Vector2(rectangle.Right, rectangle.Bottom), color, borderWidth);
    #endregion
}
