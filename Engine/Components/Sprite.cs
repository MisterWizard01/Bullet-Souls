using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Components;

public class Sprite : IComponent
{

    public Vector2 Offset { get; set; }
    public string TextureName { get; set; }
    public Dictionary<string, Animation> Animations { get; set; }

    [JsonIgnore]
    public Texture2D Texture { get; set; }
    [JsonIgnore]
    public Animation? CurrentAnimation { get; set; }
    [JsonIgnore]
    public float FrameRatio { get; set; }
    [JsonIgnore]
    public float FrameIndex { get; set; }
    [JsonIgnore]
    public Color BlendColor { get; set; }

    [JsonIgnore]
    public Point Size { get => CurrentAnimation?.Size ?? new(-1, -1); }
    //[JsonIgnore]
    //public int Width { get => CurrentAnimation?.Width ?? -1; }
    //[JsonIgnore]
    //public int Height { get => CurrentAnimation?.Height ?? -1; }

    public Sprite(Texture2D texture, float frameRatio = 1, float startIndex = 0)
    {
        Animations = new Dictionary<string, Animation>();
        Texture = texture;
        TextureName = texture.Name;
        FrameIndex = startIndex;
        FrameRatio = frameRatio;
        BlendColor = Color.White;
        Offset = Vector2.Zero;
    }

    [JsonConstructor]
    public Sprite(string texturePath, Vector2 offset, Color blendColor, float frameRatio = 1, float startIndex = 0)
    {
        Animations = new Dictionary<string, Animation>();
        TextureName = texturePath;
        FrameIndex = startIndex;
        FrameRatio = frameRatio;
        BlendColor = Color.White;
        Offset = Vector2.Zero;
    }

    public void Update(int frameNumber, InputState inputState)
    {
        if (CurrentAnimation == null)
        {
            return;
        }

        FrameIndex += FrameRatio;
        if (FrameIndex >= CurrentAnimation.Frames.Count || FrameIndex < 0)
        {
            switch (CurrentAnimation.EndAction)
            {
                case AnimationEndAction.Stop:
                    FrameRatio = 0;
                    FrameIndex = CurrentAnimation.Frames.Count - 1;
                    break;

                case AnimationEndAction.Cycle:
                    FrameIndex %= CurrentAnimation.Frames.Count;
                    break;

                case AnimationEndAction.PingPong:
                    FrameIndex -= FrameRatio * 2;
                    FrameRatio *= -1;
                    break;

                case AnimationEndAction.Reverse:
                    FrameRatio *= -1;
                    FrameIndex = CurrentAnimation.Frames.Count - 1;
                    break;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (CurrentAnimation == null)
        {
            return;
        }

        var source = CurrentAnimation[(int)FrameIndex];
        var destination = new Rectangle(
            (int)(Offset.X - CurrentAnimation.Size.X / 2.0f),
            (int)(Offset.Y - CurrentAnimation.Size.Y / 2.0f),
            CurrentAnimation.Size.X, CurrentAnimation.Size.Y
        );
        spriteBatch.Draw(Texture, destination, source, BlendColor);
    }

    public void SetAnimation(string name)
    {
        if (Animations.ContainsKey(name))
        {
            CurrentAnimation = Animations[name];
        }
        else
        {
            CurrentAnimation = null;
            Debug.WriteLine("Sprite does not have an animation called \"" + name + "\".");
        }
    }

    public Animation AddAnimation(string name, List<Point> sources, AnimationEndAction endAction = AnimationEndAction.Cycle)
    {
        var animation = new Animation()
        {
            Frames = sources,
            EndAction = endAction,
        };
        Animations.Add(name, animation);
        return animation;
    }
}
