using Engine;
using Engine.Nodes;
using Engine.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json.Linq;
using BulletSoulsLibrary;

namespace ControlsProto;

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Texture2D _background, _whitePixel;
    private Camera _camera;
    private RasterizerState _rasterizerState;
    private PlayerNode _player;
    private Node _enemies;

    private readonly InputManager _inputManager;
    private readonly JuicyContentManager _contentManager;

    private KeyboardState _prevKeyboardState;
    private int frameNumber;
    //private Effect _grayscaleEffect, _silhouetteEffect;
    private Effect _betterBlend;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnResizeWindow;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _inputManager = new(InputMode.MouseAndKeyboard);
        _contentManager = new();
    }

    protected override void Initialize()
    {
        _inputManager.SetBinding(InputMode.KeyboardOnly, InputSignal.HorizontalMovement, new KeyInput(Keys.A, Keys.D));
        _inputManager.SetBinding(InputMode.KeyboardOnly, InputSignal.VerticalMovement, new KeyInput(Keys.W, Keys.S));
        _inputManager.SetBinding(InputMode.KeyboardOnly, InputSignal.HorizontalFacing, new KeyInput(Keys.Left, Keys.Right));
        _inputManager.SetBinding(InputMode.KeyboardOnly, InputSignal.VerticalFacing, new KeyInput(Keys.Up, Keys.Down));
        _inputManager.SetBinding(InputMode.KeyboardOnly, InputSignal.Strafe, new KeyInput(Keys.LeftShift));
        _inputManager.SetBinding(InputMode.KeyboardOnly, InputSignal.Dash, new KeyInput(Keys.Space));

        _inputManager.SetBinding(InputMode.MouseAndKeyboard, InputSignal.HorizontalMovement, new KeyInput(Keys.A, Keys.D));
        _inputManager.SetBinding(InputMode.MouseAndKeyboard, InputSignal.VerticalMovement, new KeyInput(Keys.W, Keys.S));
        _inputManager.SetBinding(InputMode.MouseAndKeyboard, InputSignal.HorizontalFacing, new MouseAxisInput(MouseAxes.MouseX));
        _inputManager.SetBinding(InputMode.MouseAndKeyboard, InputSignal.VerticalFacing, new MouseAxisInput(MouseAxes.MouseY));
        _inputManager.SetBinding(InputMode.MouseAndKeyboard, InputSignal.Dash, new KeyInput(Keys.Space));
        
        _inputManager.SetBinding(InputMode.XBoxController, InputSignal.HorizontalMovement, new GamePadAxisInput(GamePadAxes.LeftStickX));
        _inputManager.SetBinding(InputMode.XBoxController, InputSignal.VerticalMovement, new GamePadAxisInput(GamePadAxes.LeftStickY, true));
        _inputManager.SetBinding(InputMode.XBoxController, InputSignal.HorizontalFacing, new GamePadAxisInput(GamePadAxes.RightStickX));
        _inputManager.SetBinding(InputMode.XBoxController, InputSignal.VerticalFacing, new GamePadAxisInput(GamePadAxes.RightStickY, true));
        _inputManager.SetBinding(InputMode.XBoxController, InputSignal.Strafe, new GamePadButtonInput(Buttons.LeftTrigger));
        _inputManager.SetBinding(InputMode.XBoxController, InputSignal.Dash, new GamePadButtonInput(Buttons.B));

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

        //_grayscaleEffect = Content.Load<Effect>("grayscale");
        //_silhouetteEffect = Content.Load<Effect>("silhouette");
        _betterBlend = Content.Load<Effect>("betterBlend");

        string commonFolder = FileManager.GetCommonFolder();
        _contentManager.LoadTextures(Content, Path.Combine(commonFolder, "Textures.json"));
        _contentManager.LoadSprites(Path.Combine(commonFolder, "Sprites.json"));
        LoadSceneFromTiled(Path.Combine(commonFolder, "tiled\\test.json"));
        _background = _contentManager.Textures["background"];
    }

    protected override void Update(GameTime gameTime)
    {
        //recieve inputs
        KeyboardState keyboardState = Keyboard.GetState();
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Escape))
            Exit();
        if (keyboardState.IsKeyDown(Keys.F1) && _prevKeyboardState.IsKeyUp(Keys.F1))
            ToggleFullScreen();
        _inputManager.Update(_camera.GameToView(_player.Position));
        var inputState = _inputManager.InputState;

        //update objects
        _player.Update(null, frameNumber, inputState);
        _enemies.Update(null, frameNumber, inputState);

        //ready next frame
        _prevKeyboardState = keyboardState;
        frameNumber++;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            rasterizerState: _rasterizerState,
            effect: _betterBlend,
            blendState: BlendState.NonPremultiplied
        );
        _spriteBatch.GraphicsDevice.ScissorRectangle = _camera.ViewRect;

        _camera.Draw(_background, _camera.GameRect, Color.Transparent);
        _player.Draw(null, _camera, Vector2.Zero);
        _enemies.Draw(null, _camera, Vector2.Zero);

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

    protected void LoadScene(string filePath)
    {
        using StreamReader reader = new(filePath);
        var json = reader.ReadToEnd();
        var scene = JsonConvert.DeserializeObject<Node>(json);
        if (scene == null)
        {
            Debug.WriteLine("Could not read JSON sprite file.");
            return;
        }

        //_player = scene.GetChild("player") as PlayerNode;
        _enemies = scene.GetChild("enemies");
    }

    protected void LoadSceneFromTiled(string filePath)
    {
        using StreamReader reader = new(filePath);
        var json = reader.ReadToEnd();
        var jObject = JObject.Parse(json);
        var scene = TiledParser.ParseMap(_contentManager, jObject);
        if (scene == null)
        {
            Debug.WriteLine("Could not read JSON sprite file.");
            return;
        }

        _player = scene.GetChild("Player").GetChild("player") as PlayerNode;
        _enemies = new Node();

        _player.Initialize();
    }
}