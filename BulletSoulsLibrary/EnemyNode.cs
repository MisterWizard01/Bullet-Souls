using Engine;
using Engine.Nodes;
using Microsoft.Xna.Framework;

namespace BulletSoulsLibrary;

public class EnemyNode : Node2D
{
    private SpriteNode? _sprite;
    public ColliderNode? Collider { get; private set; }

    public Vector2 Target { get; set; }
    public float MoveSpeed { get; set; }

    public EnemyNode(Vector2 position) : base(position)
    {
        _sprite = GetChild("sprite") as SpriteNode;
        MoveSpeed = 0.5f;
    }
    
    public EnemyNode(Vector2 position, Sprite sprite) : base(position)
    {
        _sprite = new SpriteNode(sprite, "stand down");
        AddChild("sprite", _sprite);
        Collider = new ColliderNode(0, 0, 16, 20);
        AddChild("collider", Collider);

        MoveSpeed = 0.5f;
    }

    public override void Update(Node parent, int frameNumber, InputState inputState)
    {
        base.Update(parent, frameNumber, inputState);
        var velocity = Target - Position;
        if (Math.Abs(velocity.X) < Math.Abs(velocity.Y))
            velocity = new(0, MoveSpeed * Math.Sign(velocity.Y));
        else
            velocity = new(MoveSpeed * Math.Sign(velocity.X), 0);
        Position += velocity;

    }
}
