using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public interface IComponent
    {
        public void Update(int frameNumber, InputState inputState);

        public void Draw(SpriteBatch spriteBatch);
    }
}