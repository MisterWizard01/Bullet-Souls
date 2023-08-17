using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Engine;

public class Animation
{
    private Point _size;

    public List<Point> Frames { get; set; }
    public AnimationEndAction EndAction { get; set; }
    public Point Size { get; set; }
    
    //[JsonIgnore]
    //public int Width
    //{
    //    get { return _size.X; }
    //    set { _size.X = value; }
    //}
    //[JsonIgnore]
    //public int Height
    //{
    //    get { return _size.Y; }
    //    set { _size.Y = value; }
    //}
    [JsonIgnore]
    public int Length => Frames.Count;

    public Animation()
    {
        Frames = new();
    }

    public Rectangle this[int frameIndex]
    {
        get => new(Frames[frameIndex], _size);
    }
}

//public struct Frame
//{
//    public Rectangle Source, Destination;

//    public Frame(Rectangle rect)
//    {
//        Source = rect;
//        Destination = new Rectangle(Point.Zero, rect.Size);
//    }

//    public Frame(Rectangle destination, Rectangle source)
//    {
//        Destination = destination;
//        Source = source;
//    }
//}

