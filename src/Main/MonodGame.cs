using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monod.AssetsSystem;
using Monod.GraphicsSystem;
using Monod.InputSystem;
using Monod.ModSystem;
using Monod.TimeSystem;
using Monod.Utils.General;
using MonoPlus.AssetsSystem;
using Serilog;

namespace MonoPlus;

/// <summary>
/// Class containing similar code for all apps made with MonodEngine, to reduce repetitve code. Every app must have a class inheriting from <see cref="MonodGame"/>.
/// </summary>
public abstract class MonodGame : Game
{
    /// <summary>
    /// Main <see cref="AssetManager"/> for vanilla game.
    /// </summary>
    public static AssetManager? MainAssetManager;

    /// <summary>
    /// Creates a new <see cref="MonodGame"/>.
    /// </summary>
    protected MonodGame()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        MonodMain.OnGameCreated(this);
        IsFixedTimeStep = false;
        Window.AllowUserResizing = true;
    }

    /// <inheritdoc/> 
    protected override void Initialize()
    {
        MonodMain.OnGameInitialize(this);
        base.Initialize();
    }

    /// <inheritdoc/> 
    protected override void LoadContent()
    {
        Renderer.spriteBatch = new SpriteBatch(GraphicsDevice);

        string contentPath = $"{AppContext.BaseDirectory}Content";
        MainAssetManager = new FileAssetManager(contentPath);
        if (MainAssetManager is null) throw new InvalidOperationException("Couldn't create MainAssetManager");
        Assets.RegisterAssetManager(MainAssetManager, "");
        MainAssetManager.LoadAssets();
        Log.Information("Started loading content");
    }

    /// <inheritdoc/> 
    protected override void Update(GameTime gameTime)
    {
        Time.Update(gameTime, IsActive);
        if (GraphicsSettings.FocusLossBehaviour > GraphicsSettings.OnFocusLossBehaviour.Eco && !IsActive) return;
        base.Update(gameTime);
        Input.Update();
        MainThread.Update();

        ModManager.Update();
        //DevConsole.Update(); TODO dev console in-game w/ Console class support like in DD.

        UpdateM();

        Input.PostUpdate();
    }

    /// <inheritdoc/> 
    protected override void Draw(GameTime gameTime)
    {
        if (GraphicsSettings.FocusLossBehaviour > GraphicsSettings.OnFocusLossBehaviour.Eco && !IsActive) return;
        GraphicsDevice.Clear(Color.Black);
        if (ModManager.InProgress)
        {
            //font.DrawText("Loading mods.", Renderer.spriteBatch, Vector2.Zero); TODO have some sort of default font or something
            return;
        }
        
        DrawM();
        base.Draw(gameTime);
    }

    /// <summary>
    /// Called when the game should update. Override this to update your game.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Time"/> to access delta time.
    /// </remarks>
    protected abstract void UpdateM();

    /// <summary>
    /// Called when the game should draw a frame. Override this to render your game.
    /// </summary>
    protected abstract void DrawM();
}