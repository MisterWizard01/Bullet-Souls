using Engine;
using Engine.Nodes;
using Microsoft.Xna.Framework;

namespace BulletSoulsLibrary;

public class ShotNode : Node2D
{
    private SpriteNode? _sprite;
    public Vector2 Velocity;
    public int Age;

    public ShotNode(Vector2 position, Vector2 velocity) : base(position)
    {
        Velocity = velocity;
        _sprite = GetChild("sprite") as SpriteNode;
    }

    public ShotNode(Vector2 position, Vector2 velocity, Sprite sprite) : base(position)
    {
        Velocity = velocity;
        _sprite = new SpriteNode(sprite, "main");
        AddChild("sprite", _sprite);
    }

    public override void Update(Node parent, int frameNumber, InputState inputState)
    {
        Position += Velocity;
        Age++;

        base.Update(parent, frameNumber, inputState);
    }
}
