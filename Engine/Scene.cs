using Engine.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine;

public class Scene
{
    public Dictionary<string, Sprite> Sprites;
    public Dictionary<string, GameObject[]> Objects;

    [JsonConstructor]
    public Scene(Dictionary<string, Sprite> sprites, Dictionary<string, GameObject[]> objects)
    {
        Sprites = sprites;
        Objects = objects;
    }
}
