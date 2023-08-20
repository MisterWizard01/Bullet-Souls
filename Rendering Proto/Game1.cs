using Engine;
using Engine.Components;
using Engine.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace RenderingProto;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Texture2D _texture;
    private Rectangle _viewRect, _camera;
    private GameObject _player;
    private GameObject[] _enemies;

    private InputManager _inputManager;
    private SpriteManager _spriteManager;

    private KeyboardState _prevKeyboardState;
    private int frameNumber;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnResizeWindow;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _inputManager = new(InputMode.mouseAndKeyboard);
        _spriteManager = new();
        _camera = new Rectangle(0, 0, 192, 144);
        OnResizeWindow(null, null);
    }

    protected override void Initialize()
    {
        _inputManager.SetBinding(InputSignal.HorizontalMovement, new KeyInput(Keys.A, Keys.D));
        _inputManager.SetBinding(InputSignal.VerticalMovement, new KeyInput(Keys.W, Keys.S));

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _texture = Content.Load<Texture2D>("Bullet Souls mockup 1");
        LoadScene("Data.json");
    }

    protected override void Update(GameTime gameTime)
    {
        //recieve inputs
        KeyboardState keyboardState = Keyboard.GetState();
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Escape))
            Exit();
        if (keyboardState.IsKeyDown(Keys.Space) && _prevKeyboardState.IsKeyUp(Keys.Space))
            ToggleFullScreen();
        _inputManager.Update();
        var inputState = _inputManager.InputState;

        //update objects
        _player.Update(frameNumber, inputState);
        //foreach (var shot in _playerShots)
        //{
        //    shot.Update(frameNumber, inputState);
        //}
        //foreach (var bullet in _bullets)
        //{
        //    bullet.Update(frameNumber, inputState);
        //}
        foreach (var enemy in _enemies)
        {
            enemy.Update(frameNumber, inputState);
        }

        //detect and handle collisions
        //var enemyCollisions = new List<Collision>();
        //enemyCollisions.AddRange(Collision.CheckCollisions(_enemies));
        //Collision.SortCollisions(enemyCollisions);
        //foreach (var collision in enemyCollisions)
        //{

        //}

        //ready next frame
        _prevKeyboardState = keyboardState;
        frameNumber++;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        _spriteBatch.Draw(_texture, _viewRect, Color.White);
        var playerSprite = (SpriteComponent)_player.Components["sprite"];
        _spriteManager.DrawSprite(_spriteBatch, playerSprite);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    protected void ToggleFullScreen()
    {
        _graphics.IsFullScreen = !_graphics.IsFullScreen;
        if (_graphics.IsFullScreen)
        {
            _graphics.PreferredBackBufferWidth = GraphicsDevice.Adapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsDevice.Adapter.CurrentDisplayMode.Height;
        }
        else
        {
            _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            _graphics.PreferredBackBufferWidth = Window.ClientBounds.Height;
        }
        _graphics.ApplyChanges();
        OnResizeWindow(null, null);
    }

    protected void OnResizeWindow(object sender, EventArgs e)
    {
        int xScale = Math.Max(1, Window.ClientBounds.Width / _camera.Width);
        int yScale = Math.Max(1, Window.ClientBounds.Height / _camera.Height);
        int scale = Math.Min(xScale, yScale);
        Point size = new(_camera.Width * scale, _camera.Height * scale);
        Point location = new((Window.ClientBounds.Width - size.X) / 2, (Window.ClientBounds.Height - size.Y) / 2);
        _viewRect = new Rectangle(location, size);
    }

    protected void LoadScene(string filePath)
    {
        using StreamReader reader = new(filePath);
        var json = reader.ReadToEnd();
        var scene = JsonConvert.DeserializeObject<Scene>(json);
        if (scene == null)
        {
            Debug.WriteLine("Could not read JSON sprite file.");
            return;
        }
        _spriteManager.Sprites = scene.Sprites;
        _spriteManager.LoadSpriteTextures(Content);

        _player = scene.Objects["players"][0];
        _enemies = scene.Objects["enemies"];
    }
}