using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Engine;

public class GameObject
{
    public Dictionary<string, IComponent> Components;

    public GameObject()
    {
        Components = new();
    }

    public void Update(int frameNumber, InputState keyboardState)
    {
        foreach (var component in Components.Values)
        {
            //component.Update(frameNumber, keyboardState);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var component in Components.Values)
        {
            //component.Draw(spriteBatch);
        }
    }
}