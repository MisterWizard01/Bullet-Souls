using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class Camera
    {
        public Rectangle GameRect { get; set; }
        public Rectangle ViewRect { get; set; }
        public SpriteBatch SpriteBatch { get; set; }
        
        public float Scale
        {
            get { return ViewRect.Width / GameRect.Width; }
        }

        public Camera(SpriteBatch spriteBatch, Rectangle gameRect, int scale = 1)
            : this(spriteBatch, gameRect, new Rectangle(gameRect.X, gameRect.Y, gameRect.Width * scale, gameRect.Height * scale)) { }

        public Camera(SpriteBatch spriteBatch, Rectangle gameRect, Rectangle viewRect)
        {
            SpriteBatch = spriteBatch;
            GameRect = gameRect;
            ViewRect = viewRect;
        }

        public void Draw(Texture2D texture2D, Rectangle destination, Rectangle source, Color color)
        {
            var finalPosition = GameToView(destination.Location);
            var finalDestination = new Rectangle(
                finalPosition.X,
                finalPosition.Y,
                destination.Width * (int)Scale,
                destination.Height * (int)Scale
            );
            SpriteBatch.Draw(texture2D, finalDestination, source, color);
        }

        public void Draw(Texture2D texture2D, Rectangle destination, Color color)
        {
            Draw(texture2D, destination, texture2D.Bounds, color);
        }

        public void DrawString(SpriteFont font, string text, Vector2 destination, Color color)
        {
            var finalPosition = GameToView(destination);
            SpriteBatch.DrawString(font, text, finalPosition, color, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
        }

        public Point GameToView(Point gamePosition)
        {
            var result = GameToView(gamePosition.ToVector2());
            return result.ToPoint();
        }

        public Vector2 GameToView(Vector2 gamePosition)
        {
            return (gamePosition - GameRect.Location.ToVector2()) * Scale + ViewRect.Location.ToVector2();
        }

        public Point ViewToGame(Point viewPosition)
        {
            var result = ViewToGame(viewPosition.ToVector2());
            return result.ToPoint();
        }

        public Vector2 ViewToGame(Vector2 viewPosition)
        {
            return (viewPosition - ViewRect.Location.ToVector2()) / Scale + GameRect.Location.ToVector2();
        }
    }
}
