using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoPlus.GraphicsSystem.BitmapFonts;

namespace MonoPlus.GraphicsSystem;

/// <summary>
/// Class for managing <see cref="GraphicsDeviceManager"/>, <see cref="SpriteBatch"/> and drawing.
/// </summary>
public static class Renderer
{
    /// <summary>
    /// Global <see cref="GraphicsDeviceManager"/>.
    /// </summary>
    public static GraphicsDeviceManager deviceManager = null!;

    /// <summary>
    /// Global <see cref="GraphicsDevice"/>.
    /// </summary>
    public static GraphicsDevice device = null!;
    
    /// <summary>
    /// Current <see cref="RenderTarget2D"/> for <see cref="device"/>, or <see langword="null"/> if drawing to BackBuffer. Private field for <see cref="RenderTarget"/>.
    /// </summary>
    private static RenderTarget2D? renderTarget = null;

    /// <summary>
    /// Current <see cref="RenderTarget2D"/> for <see cref="device"/>, or <see langword="null"/> if drawing to BackBuffer.
    /// </summary>
    public static RenderTarget2D? RenderTarget => renderTarget;

    /// <summary>
    /// Global <see cref="SpriteBatch"/>.
    /// </summary>
    public static SpriteBatch spriteBatch = null!;

    /// <summary>
    /// Whether <see cref="SpriteBatch.Begin"/> was called, and <see cref="SpriteBatch.End"/> wasn't yet called.
    /// </summary>
    private static bool spriteBatchActive;

    /// <summary>
    /// Whether <see cref="SpriteBatch.Begin"/> was called, and <see cref="SpriteBatch.End"/> wasn't yet called.
    /// </summary>
    public static bool SpriteBatchActive => spriteBatchActive;

    /// <summary>
    /// 1x1 white pixel texture for drawing all primitive stuff like lines and rectangles.
    /// </summary>
    public static Texture2D Pixel = null!;

    /// <summary>
    /// Window.Handle which can be used to manipulate window with SDL.
    /// </summary>
    public static IntPtr WindowHandle;

    /// <summary>
    /// <see cref="GameWindow"/> of running game.
    /// </summary>
    public static GameWindow Window = null!;


    /// <summary>
    /// Call in <see cref="Game"/>'s constructor.
    /// </summary>
    /// <param name="game">Created game.</param>
    public static void OnGameCreated(Game game)
    {
        deviceManager = new GraphicsDeviceManager(game);
    }

    /// <summary>
    /// Call in <see cref="Game.Initialize"/>.
    /// </summary>
    /// <param name="game">Initialized game.</param>
    /// <exception cref="InvalidOperationException"><see cref="deviceManager"/> is null.</exception>
    public static void Initialize(Game game)
    {
        if (deviceManager == null) throw new InvalidOperationException("deviceManager is null");
        device = game.GraphicsDevice;
        Window = game.Window;
        WindowHandle = Window.Handle;
        spriteBatch = new SpriteBatch(game.GraphicsDevice);
        Pixel = new(device, 1, 1);
        Pixel.SetData([Color.White]);
    }

    //Meta drawing functions

    /// <summary>
    /// Clears resource buffers.
    /// </summary>
    /// <param name="color">Set this color value in all buffers.</param>
    public static void Clear(Color color) => device?.Clear(color);

    
    /// <summary>
    /// Begins a new sprite and text batch with the specified render state.
    /// </summary>
    /// <param name="sortMode">The drawing order for sprite and text drawing. <see cref="SpriteSortMode.Deferred"/> by default.</param>
    /// <param name="blendState">State of the blending. Uses <see cref="BlendState.AlphaBlend"/> if null.</param>
    /// <param name="samplerState">State of the sampler. Uses <see cref="SamplerState.LinearClamp"/> if null.</param>
    /// <param name="depthStencilState">State of the depth-stencil buffer. Uses <see cref="DepthStencilState.None"/> if null.</param>
    /// <param name="rasterizerState">State of the rasterization. Uses <see cref="RasterizerState.CullCounterClockwise"/> if null.</param>
    /// <param name="effect">A custom <see cref="Effect"/> to override the default sprite effect. Uses default sprite effect if null.</param>
    /// <param name="transformMatrix">An optional matrix used to transform the sprite geometry. Uses <see cref="Matrix.Identity"/> if null.</param>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="Begin"/> is called next time without previous <see cref="End"/>.</exception>
    /// <remarks>This method uses optional parameters.</remarks>
    /// <remarks>The <see cref="Begin"/> Begin should be called before drawing commands, and you cannot call it again before subsequent <see cref="End"/>.</remarks>
    public static void Begin(SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState? blendState = null,
    SamplerState? samplerState = null, DepthStencilState? depthStencilState = null,
    RasterizerState? rasterizerState = null, Effect? effect = null, Matrix? transformMatrix = null)
    {
        spriteBatchActive = true;
        spriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);
    } 


    /// <summary>
    /// Ends <see cref="spriteBatch"/>.
    /// </summary>
    public static void End()
    {
        spriteBatchActive = false;
        spriteBatch.End();
    }

    /// <summary>
    /// Sets <see cref="renderTarget"/> and calls binds <paramref name="newRenderTarget"/> to <see cref="device"/>. Everything will be draw to <paramref name="newRenderTarget"/> until new one is set.
    /// </summary>
    /// <param name="newRenderTarget">New render target where to draw, or null to reset to BackBuffer.</param>
    /// <remarks>
    /// Use <see langword="null"/> to reset to BackBuffer. Pass <see cref="RenderTarget"/> to get current renderTarget.
    /// </remarks>
    public static void SetRenderTarget(RenderTarget2D? newRenderTarget)
    {
        renderTarget = newRenderTarget;
        device.SetRenderTarget(renderTarget);
    }

    /// <summary>
    /// Instances a new <see cref="RenderTarget2D"/> with specified options.
    /// </summary>
    /// <param name="width">Width, in pixels, of the render target.</param>
    /// <param name="height">Height, in pixels, of the render target.</param>
    /// <param name="mipMap"><b>true</b> if mipmapping is enabled; otherwise, <b>false</b>.</param>
    /// <param name="preferredFormat">The preferred surface format of the render target.</param>
    /// <param name="preferredDepthFormat">The preferred depth format of the render target.</param>
    /// <param name="preferredMultiSampleCount">The preferred number of samples per pixel when multisampling.</param>
    /// <param name="usage">The behavior to use when binding the render target to the graphics device.</param>
    /// <returns>New <see cref="RenderTarget2D"/>, instanced with specified options.</returns>
    public static RenderTarget2D CreateRenderTarget(int width, int height, bool mipMap = false, SurfaceFormat preferredFormat = SurfaceFormat.Color, DepthFormat preferredDepthFormat = DepthFormat.None, int preferredMultiSampleCount = 0, RenderTargetUsage usage = RenderTargetUsage.DiscardContents) => new(device, width, height,
    mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage);
    
    //Basic drawing functions

    /// <summary>
    /// Submit a sprite for drawing in the current batch.
    /// </summary>
    /// <param name="texture">A texture.</param>
    /// <param name="position">The drawing location on render target.</param>
    /// <param name="color">A color mask.</param>
    public static void DrawTexture(Texture2D texture, Vector2 position, Color color) => spriteBatch.Draw(texture, position, color);

    /// <summary>
    /// Submit a sprite for drawing in the current batch.
    /// </summary>
    /// <param name="texture">A texture.</param>
    /// <param name="position">The drawing location on render target.</param>
    /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
    /// <param name="color">A color mask.</param>
    /// <param name="rotation">A rotation of this sprite.</param>
    /// <param name="origin">Center of the rotation. 0,0 by default.</param>
    /// <param name="scale">A scaling of this sprite.</param>
    /// <param name="effects">Modificators for drawing. Can be combined.</param>
    /// <param name="layerDepth">A depth of the layer of this sprite.</param>
    public static void DrawTexture(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color,
    float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0) => spriteBatch.Draw(texture,
    position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);

    /// <summary>
    /// Submit a text string of sprites for drawing in the current batch.
    /// </summary>
    /// <param name="font">A font.</param>
    /// <param name="text">The text which will be drawn.</param>
    /// <param name="position">The drawing location on render target.</param>
    /// <param name="color">A color mask.</param>
    public static void DrawText(SpriteFont font, string text, Vector2 position, Color color) => spriteBatch.DrawString(font, text, position, color);

    /// <summary>
    /// Submit a text string of sprites for drawing in the current batch.
    /// </summary>
    /// <param name="font">A font.</param>
    /// <param name="text">The text which will be drawn.</param>
    /// <param name="position">The drawing location on render target.</param>
    /// <param name="color">A color mask.</param>
    /// <param name="rotation">A rotation of this string.</param>
    /// <param name="origin">Center of the rotation. 0,0 by default.</param>
    /// <param name="scale">A scaling of this string.</param>
    /// <param name="effects">Modificators for drawing. Can be combined.</param>
    /// <param name="layerDepth">A depth of the layer of this string.</param>
    /// <param name="rtl">Text is Right to Left.</param>
    public static void DrawText(SpriteFont font, string text, Vector2 position, Color color, float rotation, Vector2? origin = null, Vector2? scale = null, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0, bool rtl = false) => spriteBatch.DrawString(font, text, position, color, rotation, origin ?? Vector2.One, scale ?? Vector2.One, effects, layerDepth, rtl);

    /// <summary>
    /// Draws <paramref name="text"/> via <see cref="spriteBatch"/> with specified options using <paramref name="font"/>. <see cref="spriteBatch"/> must be active.
    /// </summary>
    /// <param name="font">Font, which is used to draw text.</param>
    /// <param name="text">Text to draw.</param>
    /// <param name="position">The drawing location on render target.</param>
    /// <param name="color">A color mask.</param>
    /// <param name="rotation">A rotation of this sprite.</param>
    /// <param name="origin">Center of the rotation. 0,0 by default.</param>
    /// <param name="scale">A scaling of this sprite.</param>
    /// <param name="effects">Modificators for drawing. Can be combined.</param>
    /// <param name="layerDepth">A depth of the layer of this sprite.</param>
    public static void DrawText(BitmapFont font, string text, Vector2 position, Color? color = null,
    float rotation = 0f, Vector2? origin = null, Vector2? scale = null, SpriteEffects effects = SpriteEffects.None,
    float layerDepth = 0) =>
        font.DrawText(text, spriteBatch, position, color, rotation, origin, scale, effects, layerDepth);


    //Drawing shapes

    /// <summary>
    /// Draws a line.
    /// </summary>
    /// <param name="lineStart">Position of one line's edge.</param>
    /// <param name="lineEnd">Position of other line's edge.</param>
    /// <param name="color">Line's color.</param>
    /// <param name="width">Line's width.</param>
    /// <exception cref="InvalidOperationException"><see cref="Pixel"/> is null</exception>
    public static void DrawLine(Vector2 lineStart, Vector2 lineEnd, Color color, float width = 1f)
    {
        if (Pixel is null) throw new InvalidOperationException("DrawLine was called, but Pixel is null");
        DrawTexture(Pixel, lineStart, null, color, (float)Math.Atan2(lineEnd.Y - lineStart.Y, lineEnd.X - lineStart.X), new Vector2(0f, 0.5f), new Vector2((lineStart - lineEnd).Length(), width));
    }

    /// <summary>
    /// Draws a rectangle.
    /// </summary>
    /// <param name="p1">Rectangle's top left corner.</param>
    /// <param name="p2">Rectangle's bottom right corner.</param>
    /// <param name="color">Rectangle's color.</param>
    public static void DrawRect(Vector2 p1, Vector2 p2, Color color)
    {
        if (Pixel is null) throw new InvalidOperationException("DrawRect was called, but Pixel is null");
        DrawTexture(Pixel, p1, null, color, 0f, Vector2.Zero, new Vector2(-(p1.X - p2.X), -(p1.Y - p2.Y)));
    }
        
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
}
