using Engine;
using Engine.Components;
using Engine.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;

namespace RenderingProto;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Texture2D _background, _tinyMonoTexture, _mostlySansTexture, _blockySansTexture, _basicallyAsepriteTexture, _whitePixel;
    private SpriteFont _tinyMono, _mostlySans, _blockySans, _basicallyAseprite;
    private Camera _camera;
    private RasterizerState _rasterizerState;
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
        _camera = new(new Rectangle(0, 0, 192, 144));
        _rasterizerState = new() { ScissorTestEnable = true };
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

        _whitePixel = new Texture2D(GraphicsDevice, 1, 1);
        _whitePixel.SetData(new Color[] { Color.White });

        _background = Content.Load<Texture2D>("Bullet Souls mockup 1");
        _tinyMonoTexture = Content.Load<Texture2D>("tiny mono");
        _mostlySansTexture = Content.Load<Texture2D>("mostly sans");
        _basicallyAsepriteTexture = Content.Load<Texture2D>("basically aseprite");
        _blockySansTexture = Content.Load<Texture2D>("blocky sans");

        _tinyMono = FontBuilder.BuildFont(_tinyMonoTexture, new Point(3, 3), new Point(1, 1), ' ', false);
        _mostlySans = FontBuilder.BuildFont(_mostlySansTexture, new Point(5, 6), new Point(1, 1), ' ', new Point(3, 5));
        _basicallyAseprite = FontBuilder.BuildFont(_basicallyAsepriteTexture, new Point(5, 7), new Point(1, 1), ' ', new Point(4, 6));
        _blockySans = FontBuilder.BuildFont(_blockySansTexture, new Point(8, 12), new Point(1, 1), ' ', new Point(6, 10));
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

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, rasterizerState: _rasterizerState);
        _spriteBatch.GraphicsDevice.ScissorRectangle = _camera.ViewRect;

        //_spriteBatch.Draw(_background, _viewRect, Color.White);
        _camera.Draw(_spriteBatch, _whitePixel, _camera.GameRect, Color.Blue);
        var text1 = FontBuilder.LimitStringWidth(_tinyMono, "This text is certainly hard to read,\n" +
            "but that's the point.", _camera.GameRect.Width);
        _camera.DrawString(_spriteBatch, _tinyMono, text1, new Vector2(_camera.GameRect.X + 1, _camera.GameRect.Y + 1), Color.White);

        var text2 = FontBuilder.LimitStringWidth(_basicallyAseprite, "This text should be easy to read,\n" +
            "because it gives explanations of menu actions.", _camera.GameRect.Width);
        _camera.DrawString(_spriteBatch, _basicallyAseprite, text2, new Vector2(_camera.GameRect.X + 1, _camera.GameRect.Y + 21), Color.White);

        var text3 = FontBuilder.LimitStringWidth(_blockySans, "This text is certainly easy to read,\n" +
            "but I still might be able to make it better.", _camera.GameRect.Width);
        _camera.DrawString(_spriteBatch, _blockySans, text3, new Vector2(_camera.GameRect.X + 1, _camera.GameRect.Y + 51), Color.White);
            
        var playerSprite = (SpriteComponent)_player.Components["sprite"];
        _spriteManager.DrawSprite(_spriteBatch, _camera, playerSprite);

        //cover area outside camera (cheat)
        //int letterboxWidth = (Window.ClientBounds.Width - _camera.ViewRect.Width) / 2;
        //int letterboxHeight = (Window.ClientBounds.Height - _camera.ViewRect.Height) / 2;
        //_spriteBatch.Draw(_whitePixel, new Rectangle(0, 0, Window.ClientBounds.Width, letterboxHeight), Color.Black);
        //_spriteBatch.Draw(_whitePixel, new Rectangle(0, Window.ClientBounds.Bottom - letterboxHeight, Window.ClientBounds.Width, letterboxHeight), Color.Black);
        //_spriteBatch.Draw(_whitePixel, new Rectangle(0, 0, letterboxWidth, Window.ClientBounds.Height), Color.Black);
        //_spriteBatch.Draw(_whitePixel, new Rectangle(Window.ClientBounds.Right - letterboxWidth, 0, letterboxWidth, Window.ClientBounds.Height), Color.Black);

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

    protected void OnResizeWindow(object sender, EventArgs e)
    {
        int xScale = Math.Max(1, Window.ClientBounds.Width / _camera.GameRect.Width);
        int yScale = Math.Max(1, Window.ClientBounds.Height / _camera.GameRect.Height);
        int scale = Math.Min(xScale, yScale);
        Point size = new(_camera.GameRect.Width * scale, _camera.GameRect.Height * scale);
        Point location = new((Window.ClientBounds.Width - size.X) / 2, (Window.ClientBounds.Height - size.Y) / 2);
        _camera.ViewRect = new Rectangle(location, size);
    }
}