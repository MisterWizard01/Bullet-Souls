using Microsoft.Xna.Framework;

namespace Engine;

public class Animation
{
    public List<Frame> Frames { get; set; }
    public AnimationEndAction EndAction { get; set; }

    public Animation()
    {
        Frames = new();
    }

    public Frame this[int frame]
    {
        get => Frames[frame];
        set => Frames[frame] = value;
    }
}

public struct Frame
{
    public Rectangle Source, Destination;

    public Frame(Rectangle rect)
    {
        Source = rect;
        Destination = new Rectangle(Point.Zero, rect.Size);
    }

    public Frame(Rectangle destination, Rectangle source)
    {
        Destination = destination;
        Source = source;
    }
}

