using Engine.Components;
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
    }

    public void LoadSpriteTextures(ContentManager content)
    {
        foreach (var sprite in Sprites.Values)
        {
            sprite.Texture = content.Load<Texture2D>(sprite.TextureName);
        }
    }

    public void UpdateSpriteComponent(SpriteComponent sc)
    {
        if (sc.SpriteName == null)
        {
            Debug.WriteLine("Tried to update sprite component with null sprite.");
            return;
        }
        if (!Sprites.TryGetValue(sc.SpriteName, out var sprite))
            return;
        
        if (sc.AnimationName == null)
            return;
        if (!sprite.Animations.TryGetValue(sc.AnimationName, out var animation))
            return;

        sc.FrameIndex += sc.FrameRatio;
        if (sc.FrameIndex >= animation.Frames.Count || sc.FrameIndex < 0)
        {
            switch (animation.EndAction)
            {
                case AnimationEndAction.Stop:
                    sc.FrameRatio = 0;
                    sc.FrameIndex = animation.Frames.Count - 1;
                    break;

                case AnimationEndAction.Cycle:
                    sc.FrameIndex %= animation.Frames.Count;
                    break;

                case AnimationEndAction.PingPong:
                    sc.FrameIndex -= sc.FrameRatio * 2;
                    sc.FrameRatio *= -1;
                    break;

                case AnimationEndAction.Reverse:
                    sc.FrameRatio *= -1;
                    sc.FrameIndex = animation.Frames.Count - 1;
                    break;
            }
        }
    }

    public void DrawSprite(SpriteBatch spriteBatch, SpriteComponent sc)
    {
        if (sc.SpriteName == null)
        {
            Debug.WriteLine("Tried to draw null sprite.");
            return;
        }
        if (!Sprites.TryGetValue(sc.SpriteName, out var sprite))
        {
            Debug.WriteLine("Tried to draw unknown sprite '" + sc.SpriteName + "'.");
            return;
        }

        if (sc.AnimationName == null)
            return;
        if (!sprite.Animations.TryGetValue(sc.AnimationName, out var animation))
            return;

        var source = animation[(int)sc.FrameIndex];
        var destination = new Rectangle(
            (int)(sc.Offset.X - animation.Size.X / 2.0f),
            (int)(sc.Offset.Y - animation.Size.Y / 2.0f),
            animation.Size.X, animation.Size.Y
        );
        spriteBatch.Draw(sprite.Texture, destination, source, sc.BlendColor);
    }
}
