using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Engine
{
    [JsonConverter(typeof(ComponentConverter))]
    public interface IComponent
    {
        //public void Update(int frameNumber, InputState inputState);

        //public void Draw(SpriteBatch spriteBatch);
    }
}