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
    public Dictionary<string, GameObject[]> Objects { get; set; }

    [JsonConstructor]
    public Scene(Dictionary<string, GameObject[]> objects)
    {
        Objects = objects;
    }
}
