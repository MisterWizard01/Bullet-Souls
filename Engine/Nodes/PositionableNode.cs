using Microsoft.Xna.Framework;

namespace Engine.Nodes;

public class PositionableNode : Node
{
    private Vector2 _position;
    public bool RelativePosition { get; set; }

    public PositionableNode() : this(Vector2.Zero, new()) { }
    public PositionableNode(Dictionary<string, Node> children) : this(Vector2.Zero, children) { }
    public PositionableNode(Vector2 position) : this(position, new()) { }
    public PositionableNode(Vector2 position, Dictionary<string, Node> children) : base(children)
    {
        Position = position;
    }

    public Vector2 Position { get => _position; set => _position = value; }
    public float X { get => _position.X; set => _position.X = value; }
    public float Y { get => _position.Y; set => _position.Y = value; }

    public Vector2 WorldPosition(Vector2 parentPosition)
    {
        if (RelativePosition)
            return parentPosition + _position;
        return _position;
    }
    public Vector2 WorldPosition(Node parent)
    {
        if (parent is PositionableNode pNode)
            return WorldPosition(pNode.Position);
        return WorldPosition(Vector2.Zero);
    }
}
