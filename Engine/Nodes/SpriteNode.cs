using Engine.CustomEventArgs;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Engine.Nodes;

public class SpriteNode : PositionableNode
{
    private float _frameIndex;

    public float FrameRatio { get; set; }
    public Color BlendColor { get; set; }
    public bool Visible { get; set; }
    public float FrameIndex
    {
        get => _frameIndex;
        set
        {
            if (Animation != null && value >= Animation.Length)
            {
                _frameIndex = 0;
                return;
            }
            _frameIndex = MathF.Max(value, 0);
        }
    }

    [JsonIgnore]
    public Sprite? Sprite { get; set; }
    [JsonIgnore]
    public Animation? Animation { get; private set; }
    [JsonIgnore]
    public string AnimationName { get; private set; }

    /// <summary>
    /// Signals that this SpriteComponent needs its Sprite updated.
    /// </summary>
    public static event EventHandler<SpriteUpdateEventArgs> SpriteUpdateEvent;

    public SpriteNode(Sprite sprite) : this(new(), sprite) { }
    public SpriteNode(Dictionary<string, Node> children, Sprite sprite)
        : this(children)
    {
        Sprite = sprite;
    }

    public SpriteNode(string spriteName, string animationName) : this(new(), spriteName, animationName) { }

    [JsonConstructor]
    public SpriteNode(Dictionary<string, Node> children, string spriteName, string animationName)
        : this(children)
    {
        SpriteUpdateEvent(this, new(spriteName));
        SetAnimation(animationName);
    }

    private SpriteNode(Dictionary<string, Node> children)
        : base(children)
    {
        FrameRatio = 1;
        BlendColor = Color.Transparent;
        Visible = true;
        RelativePosition = true;
    }

    public override void Update(Node parent, int frameNumber, InputState inputState)
    {
        if (Sprite == null || Animation == null)
            return;

        _frameIndex += FrameRatio;
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

        base.Update(this, frameNumber, inputState);
    }

    public override void Draw(Node parent, Camera camera)
    {
        Draw(camera, (parent as PositionableNode)?.Position ?? Vector2.Zero);
        base.Draw(this, camera);
    }

    public void Draw(Camera camera, Vector2 position)
    {
        if (!Visible || Sprite == null || Sprite.Texture == null || Animation == null)
            return;

        if (!RelativePosition)
        {
            position = Vector2.Zero;
        }

        var source = Animation[(int)FrameIndex];
        var destination = new Rectangle(
            (int)(X + position.X - Animation.Size.X / 2.0f),
            (int)(Y + position.Y - Animation.Size.Y / 2.0f),
            Animation.Size.X, Animation.Size.Y
        );
        camera.Draw(Sprite.Texture, destination, source, BlendColor);
        base.Draw(this, camera);
    }

    public void SetAnimation(string animationName)
    {
        if (Sprite == null)
        {
            Debug.WriteLine("Tried to set animation for null sprite.");
            return;
        }
        if (!Sprite.Animations.TryGetValue(animationName, out Animation? animation))
        {
            Debug.WriteLine("Sprite does not contain an animation called '" + animationName + "'.");
        }
        if (Animation == animation)
            return;

        Animation = animation;
        AnimationName = animationName;
        FrameIndex = _frameIndex;
    }
}
