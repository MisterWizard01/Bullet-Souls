using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Engine;

public class Sprite
{
    public Dictionary<string, Animation> Animations { get; set; }
    public string TextureName;

    [JsonIgnore]
    public Texture2D? Texture { get; set; }
    
    public Sprite(string textureName)
    {
        Animations = new();
        TextureName = textureName;
    }
}
