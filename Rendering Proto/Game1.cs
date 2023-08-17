using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Rendering_Proto
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private SpriteManager _spriteManager;

        private Texture2D _texture;
        private Rectangle _viewRect, _camera;

        private KeyboardState _prevKeyboardState;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnResizeWindow;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _camera = new Rectangle(0, 0, 192, 144);
            OnResizeWindow(null, null);

            _spriteManager = new();
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _texture = Content.Load<Texture2D>("Bullet Souls mockup 1");
            _spriteManager.LoadSprites("Data.json");
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Escape))
                Exit();

            if (keyboardState.IsKeyDown(Keys.Space) && _prevKeyboardState.IsKeyUp(Keys.Space))
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

            _prevKeyboardState = keyboardState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            _spriteBatch.Draw(_texture, _viewRect, Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
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
    }
}