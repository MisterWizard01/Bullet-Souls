using Engine.Components;
using Engine.CustomEventArgs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Engine.Managers;

public class SpriteManager
{
    public Dictionary<string, Sprite> Sprites { get; set; }

    public SpriteManager()
    {
        Sprites = new();
        SpriteComponent.SpriteUpdateEvent += OnSpriteUpdate;
    }

    public void LoadSpriteTextures(ContentManager content)
    {
        foreach (var sprite in Sprites.Values)
        {
            sprite.Texture = content.Load<Texture2D>(sprite.TextureName);
        }
    }

    public void OnSpriteUpdate(object? sender, SpriteUpdateEventArgs e)
    {
        if (sender is not SpriteComponent sc)
            return;
        if (!Sprites.TryGetValue(e.SpriteName, out var sprite))
        {
            Debug.WriteLine("Couldn't find a sprite called '" + e.SpriteName + "'.");
        }
        sc.Sprite = sprite;
    }
}
