using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Components;

public class Sprite : IComponent
{
    private Dictionary<string, Animation> _animations;

    public Vector2 Offset { get; set; }
    public Texture2D Texture { get; set; }
    public float FrameRatio { get; set; }
    public float FrameIndex { get; set; }
    public Animation? CurrentAnimation { get; set; }
    public Color BlendColor { get; set; }

    public Frame? CurrentFrame => CurrentAnimation?.Frames[(int)FrameIndex];
    public int Width { get => CurrentFrame?.Destination.Width ?? -1; }
    public int Height { get => CurrentFrame?.Destination.Height ?? -1; }

    public Sprite(Texture2D texture, float frameRatio = 1, float startIndex = 0)
    {
        _animations = new Dictionary<string, Animation>();
        Texture = texture;
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
        if (CurrentFrame == null)
        {
            return;
        }

        var frame = CurrentFrame.Value;
        var finalDestination = new Rectangle(
            (int)(Offset.X + frame.Destination.X - frame.Destination.Width / 2.0f),
            (int)(Offset.Y + frame.Destination.Y - frame.Destination.Height / 2.0f),
            frame.Destination.Width,
            frame.Destination.Height
        );
        spriteBatch.Draw(Texture, finalDestination, frame.Source, BlendColor);
    }

    public void SetAnimation(string name)
    {
        if (_animations.ContainsKey(name))
        {
            CurrentAnimation = _animations[name];
        }
        else
        {
            CurrentAnimation = null;
            Debug.WriteLine("Sprite does not have an animation called \"" + name + "\".");
        }
    }

    public Animation AddAnimation(string name, List<Rectangle> destinations, List<Rectangle> sources, AnimationEndAction endAction = AnimationEndAction.Cycle)
    {
        if (destinations.Count != sources.Count)
        {
            throw new ArgumentException("Destination and source arrays must have the same length.");
        }

        var animation = new Animation();
        for (int i = 0; i < destinations.Count; i++)
        {
            animation.Frames.Add(new Frame(destinations[i], sources[i]));
        }
        animation.EndAction = endAction;
        _animations.Add(name, animation);
        return animation;
    }

    public Animation AddAnimation(string name, List<Rectangle> sources, AnimationEndAction endAction = AnimationEndAction.Cycle)
    {
        var animation = new Animation();
        for (int i = 0; i < sources.Count; i++)
        {
            animation.Frames.Add(new Frame(sources[i]));
        }
        animation.EndAction = endAction;
        _animations.Add(name, animation);
        return animation;
    }
}
