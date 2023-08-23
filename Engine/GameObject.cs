using Engine.Components;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Engine;

public class GameObject : Positionable
{
    public Dictionary<string, IComponent> Components;

    public GameObject()
    {
        Components = new();
    }

    public void Update(int frameNumber, InputState inputState)
    {
        foreach (var component in Components.Values)
        {
            component.Update(this, frameNumber, inputState);
        }
    }

    public void Draw(Camera camera)
    {
        foreach (var component in Components.Values)
        {
            component.Draw(this, camera);
        }
    }
}