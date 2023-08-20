using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Engine;

public class Animation
{
    public List<Point> Frames { get; set; }
    public AnimationEndAction EndAction { get; set; }
    public Point Size { get; set; }

    [JsonIgnore]
    public int Width
    {
        get { return Size.X; }
        set { Size = new(value, Height); }
    }
    [JsonIgnore]
    public int Height
    {
        get { return Size.Y; }
        set { Size = new(Width, value); }
    }
    [JsonIgnore]
    public int Length => Frames.Count;

    public Animation()
    {
        Frames = new();
    }

    public Rectangle this[int frameIndex]
    {
        get => new(Frames[frameIndex], Size);
    }
}
