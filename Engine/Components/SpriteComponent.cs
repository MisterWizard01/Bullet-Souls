using Engine.CustomEventArgs;
using Engine.JsonConverters;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Engine.Components;

//[JsonConverter(typeof(SpriteComponentConverter))]
public class SpriteComponent : Positionable, IComponent
{
    private Animation? _animation;

    public float FrameRatio { get; set; }
    public float FrameIndex { get; set; }
    public Color BlendColor { get; set; }

    [JsonIgnore]
    public Sprite? Sprite { get; set; }
    [JsonIgnore]
    public Animation? Animation
    {
        get => _animation;
        set
        {
            if (Sprite == null || value == null || !Sprite.Animations.ContainsValue(value))
            {
                _animation = null;
                return;
            }
            _animation = value;
        }
    }

    /// <summary>
    /// Signals that this SpriteComponent needs its Sprite updated.
    /// </summary>
    public static event EventHandler<SpriteUpdateEventArgs> SpriteUpdateEvent;

    public SpriteComponent(Sprite sprite)
        : this()
    {
        Sprite = sprite;
    }

    [JsonConstructor]
    public SpriteComponent(string? spriteName, string? animationName)
        : this()
    {
        if (spriteName == null)
            return;
        SpriteUpdateEvent(this, new(spriteName));
        SetAnimation(animationName);
    }

    private SpriteComponent()
    {
        FrameRatio = 1;
        BlendColor = Color.White;
    }

    public void Update(int frameNumber, InputState inputState)
    {
        if (Sprite == null)
        {
            Debug.WriteLine("Tried to update sprite component with null sprite.");
            return;
        }
        if (Animation == null)
        {
            Debug.WriteLine("Tried to update sprite component with null animation.");
            return;
        }

        FrameIndex += FrameRatio;
        if (FrameIndex >= Animation.Frames.Count || FrameIndex < 0)
        {
            switch (Animation.EndAction)
            {
                case AnimationEndAction.Stop:
                    FrameRatio = 0;
                    FrameIndex = Animation.Frames.Count - 1;
                    break;

                case AnimationEndAction.Cycle:
                    FrameIndex %= Animation.Frames.Count;
                    break;

                case AnimationEndAction.PingPong:
                    FrameIndex -= FrameRatio * 2;
                    FrameRatio *= -1;
                    break;

                case AnimationEndAction.Reverse:
                    FrameRatio *= -1;
                    FrameIndex = Animation.Frames.Count - 1;
                    break;
            }
        }
    }

    public void Draw(GameObject gameObject, Camera camera)
    {
        Draw(camera, gameObject.Position);
    }

    public void Draw(Camera camera, Vector2 referencePoint)
    {
        if (Sprite == null)
        {
            Debug.WriteLine("Tried to draw null sprite.");
            return;
        }
        if (Sprite.Texture == null)
        {
            Debug.WriteLine("Tried to draw sprite with null texture.");
            return;
        }
        if (Animation == null)
        {
            Debug.WriteLine("Tried to draw sprite with null animation.");
            return;
        }

        var source = Animation[(int)FrameIndex];
        var destination = new Rectangle(
            (int)(X + referencePoint.X - Animation.Size.X / 2.0f),
            (int)(Y + referencePoint.Y - Animation.Size.Y / 2.0f),
            Animation.Size.X, Animation.Size.Y
        );
        camera.Draw(Sprite.Texture, destination, source, BlendColor);
    }

    public void SetAnimation(string? animationName)
    {
        if (Sprite == null)
        {
            Debug.WriteLine("Tried to set animation for null sprite.");
            return;
        }
        if (animationName == null)
        {
            Animation = null;
            return;
        }
        if (!Sprite.Animations.TryGetValue(animationName, out Animation? animation))
        {
            Debug.WriteLine("Sprite does not contain an animation called '" + animationName + "'.");
        }

        Animation = animation;
    }
}
