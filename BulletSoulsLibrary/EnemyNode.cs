using Engine;
using Engine.Nodes;
using Microsoft.Xna.Framework;

namespace BulletSoulsLibrary;

public class EnemyNode : ColliderNode
{
    private SpriteNode? _sprite;
    
    public Vector2 Target { get; set; }
    public float MoveSpeed { get; set; }

    public EnemyNode(Vector2 position) : base(position, new(16, 16))
    {
        _sprite = GetChild("sprite") as SpriteNode;
        MoveSpeed = 0.5f;
    }
    
    public EnemyNode(Vector2 position, Sprite sprite) : base(position, new(16, 16))
    {
        _sprite = new SpriteNode(sprite, "stand down");
        AddChild("sprite", _sprite);

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
