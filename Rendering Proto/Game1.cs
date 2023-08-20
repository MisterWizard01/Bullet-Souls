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

        private Texture2D _background, _tinyMonoTexture, _mostlySansTexture, _blockySansTexture, _whitePixel;
        private SpriteFont _tinyMono, _mostlySans, _blockySans;
        private Camera _camera;

        private KeyboardState _prevKeyboardState;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnResizeWindow;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _camera = new Camera(new Rectangle(0, 0, 192, 144));
            OnResizeWindow(null, null);
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _whitePixel = new Texture2D(GraphicsDevice, 1, 1);
            _whitePixel.SetData(new Color[] { Color.White });

            _background = Content.Load<Texture2D>("Bullet Souls mockup 1");
            _tinyMonoTexture = Content.Load<Texture2D>("tiny mono");
            _mostlySansTexture = Content.Load<Texture2D>("mostly Sans");
            _blockySansTexture = Content.Load<Texture2D>("blocky Sans");

            _tinyMono = FontBuilder.BuildFont(_tinyMonoTexture, new Point(3, 3), new Point(1, 1), ' ', false);
            _mostlySans = FontBuilder.BuildFont(_mostlySansTexture, new Point(5, 6), new Point(1, 1), ' ', new Point(3, 5));
            _blockySans = FontBuilder.BuildFont(_blockySansTexture, new Point(8, 12), new Point(1, 1), ' ', new Point(6, 10));
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

            //_spriteBatch.Draw(_background, _viewRect, Color.White);
            _camera.Draw(_spriteBatch, _whitePixel, _camera.GameRect, Color.Blue);
            _camera.DrawString(_spriteBatch, _tinyMono, FontBuilder.LimitStringWidth(_tinyMono, "This text is certainly hard to read,\nbut that's the point.",
                _camera.GameRect.Width), new Vector2(_camera.GameRect.X + 1, _camera.GameRect.Y + 1), Color.White);
            _camera.DrawString(_spriteBatch, _mostlySans, FontBuilder.LimitStringWidth(_mostlySans, "This text should be easy to read,\nbecause it gives explanations of menu actions.",
                _camera.GameRect.Width), new Vector2(_camera.GameRect.X + 1, _camera.GameRect.Y + 21), Color.White);
            _camera.DrawString(_spriteBatch, _blockySans, FontBuilder.LimitStringWidth(_blockySans, "This text is certainly easy to read,\nbut I still might be able to make it better.",
                _camera.GameRect.Width), new Vector2(_camera.GameRect.X + 1, _camera.GameRect.Y + 41), Color.White);
            
            _spriteBatch.End();

            base.Draw(gameTime);
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
}