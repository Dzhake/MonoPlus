# MonoPlus
Sorry, I'm bad at naming things. This has nothing to do with mono. This is my personal set of libraries related to MonoGame.

# Usage
[Use as submodule / clone it folder next to your game (app)'s folder (recommended)].

# Usage requirements

## MonoPlus.Graphics

`Game.ctor`:
`Renderer.OnGameCreated(this);`

`Game.Initialize`:
`Renderer.Initialize(this);`

`Game.LoadContent`:
`Renderer.spriteBatch = new SpriteBatch(GraphicsDevice);`

`Game.Update`: (at very start)
`if (GraphicsSettings.PauseOnFocusLoss && !IsActive) return;`

`Game.Draw`: (at very start)
if (GraphicsSettings.PauseOnFocusLoss && !IsActive) return;

## MonoPlus.Input

`Game.Initialize`:
`Input.Initialize(this);`

`Game.Update`:
```cs
Input.Update();
// your logic here
Input.PostUpdate();
```

## MonoPlus.Time

`Game.Update`:
`Time.Update(gameTime);` // if used with MonoPlus.Input, `Time.Update` should be between `Input.Update` and `Input.PostUpdate`.
