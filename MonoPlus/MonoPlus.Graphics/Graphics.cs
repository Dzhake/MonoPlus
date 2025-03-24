using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoPlus.Graphics;

public static class Graphics
{
    public static GraphicsDeviceManager? deviceManager;
    public static SpriteBatch? spriteBatch;
    public static GraphicsDevice? device;
    public static Texture2D? Pixel;

    public static void OnGameCreated(Game game)
    {
        deviceManager = new GraphicsDeviceManager(game);
    }

    public static void Initialize(Game game)
    {
        if (deviceManager == null) throw new InvalidOperationException("deviceManager is null!");
        device = game.GraphicsDevice;
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

    #region AdvancedFunctions
    public static void DrawLine(Vector2 p1, Vector2 p2, Color color, float width = 1f)
    {
        if (Pixel is null) throw new InvalidOperationException("DrawLine was called, but Pixel is null!");
        DrawTexture(Pixel, p1, new Rectangle?(), color, (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X), new Vector2(0f, 0.5f), new Vector2((p1 - p2).Length(), width));
    }

    public static void DrawRect(Vector2 p1, Vector2 p2, Color col, bool filled = true, float borderWidth = 1f)
    {
        if (Pixel is null) throw new InvalidOperationException("DrawRect was called, but Pixel is null!");
        if (filled)
            DrawTexture(Pixel, p1, new Rectangle?(), col, 0f, Vector2.Zero, new Vector2(-(p1.X - p2.X), -(p1.Y - p2.Y)));
        else
        {
            float num = borderWidth / 2f;
            DrawLine(new Vector2(p1.X, p1.Y + num), new Vector2(p2.X, p1.Y + num), col, borderWidth);
            DrawLine(new Vector2(p1.X + num, p1.Y + borderWidth), new Vector2(p1.X + num, p2.Y - borderWidth), col, borderWidth);
            DrawLine(new Vector2(p2.X, p2.Y - num), new Vector2(p1.X, p2.Y - num), col, borderWidth);
            DrawLine(new Vector2(p2.X - num, p2.Y - borderWidth), new Vector2(p2.X - num, p1.Y + borderWidth), col, borderWidth);
        }
    }
    #endregion
}
