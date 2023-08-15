using Engine.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine;

public class SpriteManager
{
    public List<Sprite> Sprites { get; set; }

    public SpriteManager()
    {
        Sprites = new();
    }

    public void LoadSprites(string filePath)
    {
        using StreamReader reader = new(filePath);
        var json = reader.ReadToEnd();
        var sprites = JsonConvert.DeserializeObject<List<Sprite>>(json);
        if (sprites != null)
        {
            Debug.WriteLine("Could not read JSON sprite file.");
            return;
        }
        Sprites = sprites;
    }
}
