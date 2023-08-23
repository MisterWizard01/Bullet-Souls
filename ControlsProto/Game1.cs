using Engine;
using Engine.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ControlsProto;

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Texture2D _background, _whitePixel;
    private Camera _camera;
    private RasterizerState _rasterizerState;
    private GameObject _player;
    private GameObject[] _enemies;

    private readonly InputManager _inputManager;
    private readonly SpriteManager _spriteManager;

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
    }

    protected override void Initialize()
    {
        _inputManager.SetBinding(InputMode.mouseAndKeyboard, InputSignal.HorizontalMovement, new KeyInput(Keys.A, Keys.D));
        _inputManager.SetBinding(InputMode.mouseAndKeyboard, InputSignal.VerticalMovement, new KeyInput(Keys.W, Keys.S));
        _inputManager.SetBinding(InputMode.mouseAndKeyboard, InputSignal.HorizontalFacing, new MouseAxisInput(MouseAxes.MouseX));
        _inputManager.SetBinding(InputMode.mouseAndKeyboard, InputSignal.VerticalFacing, new MouseAxisInput(MouseAxes.MouseY));

        _inputManager.SetBinding(InputMode.XBoxController, InputSignal.HorizontalMovement, new GamePadAxisInput(GamePadAxes.LeftStickX));
        _inputManager.SetBinding(InputMode.XBoxController, InputSignal.VerticalMovement, new GamePadAxisInput(GamePadAxes.LeftStickY, true));
        _inputManager.SetBinding(InputMode.XBoxController, InputSignal.HorizontalFacing, new GamePadAxisInput(GamePadAxes.RightStickX));
        _inputManager.SetBinding(InputMode.XBoxController, InputSignal.VerticalFacing, new GamePadAxisInput(GamePadAxes.RightStickY, true));

        base.Initialize();

        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _camera = new(_spriteBatch, new Rectangle(0, 0, 192, 144));
        _rasterizerState = new() { ScissorTestEnable = true };
        OnResizeWindow(null, null);
    }

    protected override void LoadContent()
    {
        _whitePixel = new Texture2D(GraphicsDevice, 1, 1);
        _whitePixel.SetData(new Color[] { Color.White });

        _background = Content.Load<Texture2D>("Bullet Souls mockup 1");

        string commonFolder = FileManager.GetCommonFolder();
        LoadSprites(Path.Combine(commonFolder, "Data.json"));
        LoadScene(Path.Combine(commonFolder, "map.json"));
    }

    protected override void Update(GameTime gameTime)
    {
        //recieve inputs
        KeyboardState keyboardState = Keyboard.GetState();
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Escape))
            Exit();
        if (keyboardState.IsKeyDown(Keys.Space) && _prevKeyboardState.IsKeyUp(Keys.Space))
            ToggleFullScreen();
        _inputManager.Update(_camera.GameToView(_player.Position));
        var inputState = _inputManager.InputState;

        //var signals = inputState.GetInputs();
        //foreach (var signal in signals)
        //{
        //    Debug.Write(signal + " ");
        //}
        //Debug.WriteLine("");

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
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, rasterizerState: _rasterizerState);
        _spriteBatch.GraphicsDevice.ScissorRectangle = _camera.ViewRect;

        _camera.Draw(_background, _camera.GameRect, Color.White);
        _player.Draw(_camera);
        foreach (var enemy in _enemies)
        {
            enemy.Draw(_camera);
        }

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
        int xScale = Math.Max(1, Window.ClientBounds.Width / _camera.GameRect.Width);
        int yScale = Math.Max(1, Window.ClientBounds.Height / _camera.GameRect.Height);
        int scale = Math.Min(xScale, yScale);
        Point size = new(_camera.GameRect.Width * scale, _camera.GameRect.Height * scale);
        Point location = new((Window.ClientBounds.Width - size.X) / 2, (Window.ClientBounds.Height - size.Y) / 2);
        _camera.ViewRect = new Rectangle(location, size);
    }

    protected void LoadSprites(string filePath)
    {
        using StreamReader reader = new(filePath);
        var json = reader.ReadToEnd();
        var sprites = JsonConvert.DeserializeObject<Dictionary<string, Sprite>>(json);
        if (sprites == null)
        {
            Debug.WriteLine("Could not read JSON sprite file.");
            return;
        }
        _spriteManager.Sprites = sprites;
        _spriteManager.LoadSpriteTextures(Content);
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

        _player = scene.Objects["players"][0];
        _enemies = scene.Objects["enemies"];
    }
}