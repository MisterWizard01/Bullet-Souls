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
        public Rectangle GameRect, ViewRect;
        
        public float Scale
        {
            get { return ViewRect.Width / GameRect.Width; }
        }

        public Camera(Rectangle gameRect, int scale = 1)
            : this(gameRect, new Rectangle(gameRect.X, gameRect.Y, gameRect.Width * scale, gameRect.Height * scale)) { }

        public Camera(Rectangle gameRect, Rectangle viewRect)
        {
            GameRect = gameRect;
            ViewRect = viewRect;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture2D, Rectangle destination, Rectangle source, Color color)
        {
            Rectangle finalDestination = new Rectangle(
                (destination.X - GameRect.X) * (int)Scale + ViewRect.X,
                (destination.Y - GameRect.Y) * (int)Scale + ViewRect.Y,
                destination.Width * (int)Scale,
                destination.Height * (int)Scale
            );
            source.Width = finalDestination.Width;
            source.Height = finalDestination.Height;
            spriteBatch.Draw(texture2D, Rectangle.Intersect(finalDestination, ViewRect), source, color);
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture2D, Rectangle destination, Color color)
        {
            Draw(spriteBatch, texture2D, destination, texture2D.Bounds, color);
        }

        public void DrawString(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 destination, Color color)
        {
            var finalPosition = new Vector2(
                (destination.X - GameRect.X) * (int)Scale + ViewRect.X,
                (destination.Y - GameRect.Y) * (int)Scale + ViewRect.Y
            );
            spriteBatch.DrawString(font, text, finalPosition, color, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
        }
    }
}
