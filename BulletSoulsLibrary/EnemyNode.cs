using Engine;
using Engine.Nodes;
using Microsoft.Xna.Framework;

namespace BulletSoulsLibrary;

public class EnemyNode : Node2D
{
    private SpriteNode? _sprite;
    private ColliderNode? _collider;

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
        _collider = new ColliderNode(0, 0, 16, 20);
        AddChild("collider", _collider);

        MoveSpeed = 0.5f;
    }

    public override void Update(Node parent, int frameNumber, InputState inputState)
    {
        Velocity = Target - Position;
        if (Math.Abs(Velocity.X) < Math.Abs(Velocity.Y))
            Velocity = new(0, MoveSpeed * Math.Sign(Velocity.Y));
        else
            Velocity = new(MoveSpeed * Math.Sign(Velocity.X), 0);
        
        base.Update(parent, frameNumber, inputState);
    }
}
