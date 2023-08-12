using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public interface iComponent
    {
        public void Update(int frameNumber, InputState keyboardState);

        public void Draw(SpriteBatch spriteBatch);
    }
}