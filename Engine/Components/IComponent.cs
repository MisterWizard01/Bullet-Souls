using Engine.JsonConverters;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Engine.Components
{
    [JsonConverter(typeof(ComponentConverter))]
    public interface IComponent
    {
        public void Update(GameObject gameObject, int frameNumber, InputState inputState);

        public void Draw(GameObject gameObject, Camera camera);
    }
}