using Microsoft.Xna.Framework.Graphics;

namespace Engine;

public class GameObject
{
    public List<iComponent> Components;

    public GameObject()
    {
        Components = new();
    }

    public void Update(int frameNumber, InputState keyboardState)
    {
        foreach (var component in Components)
        {
            component.Update(frameNumber, keyboardState);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var component in Components)
        {
            component.Draw(spriteBatch);
        }
    }
}