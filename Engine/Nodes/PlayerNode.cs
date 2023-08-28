using Engine.Managers;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Engine.Nodes;

public class PlayerNode : PositionableNode
{
    private Vector2 _prevMove;
    private InputState _prevState;
    private int _slowFrames;
    private bool _dashFrame;
    private SpriteNode? _sprite;
    private SpriteNode?[] _dashTrail;

    public float Speed { get; set; }
    public Vector2 Facing { get; set; }
    public float WeaponCharge { get; set; }
    public float DashDistance { get; set; }
    public int DashCooldown { get; set; }

    public PlayerNode(Dictionary<string, Node> children)
        : base(children)
    {
        Facing = new(-1, 1);
        Facing.Normalize();
        _prevState = new();
        _sprite = children["sprite"] as SpriteNode;
        _dashTrail = new SpriteNode[4];
        for (int i = 0; i < 4; i++)
        {
            _dashTrail[i] = children["dashTrail" + i] as SpriteNode;
        }
    }

    public override void Update(Node parent, int frameNumber, InputState inputState)
    {
        var pressingDash = inputState[InputSignal.Dash] > 0;
        var holdingDash = _prevState[InputSignal.Dash] > 0;
        var moveVector = SpeedThisFrame(pressingDash, holdingDash) * Get2DInputVector(inputState, InputSignal.HorizontalMovement, InputSignal.VerticalMovement, true);
        var facingVector = Get2DInputVector(inputState, InputSignal.HorizontalFacing, InputSignal.VerticalFacing, true);
        Move(moveVector);

        //Update Facing
        if (facingVector.LengthSquared() == 0 && inputState[InputSignal.Strafe] == 0)
        {
            facingVector = moveVector;
        }
        if (facingVector.LengthSquared() > 0)
        {
            Facing = Vector2.Normalize(facingVector);
        }

        UpdateAnimations(moveVector.Length());
        if (_dashFrame)
        {
            LeaveDashTrail(moveVector);
        }
        UpdateDashTrail();

        _prevState = inputState;
        _dashFrame = false;
        base.Update(this, frameNumber, inputState);
    }

    public override void Draw(Node parent, Camera camera)
    {
        base.Draw(this, camera);

        //TODO: maybe draw an input display for debugging?
        //TODO: draw hud
    }

    public static Vector2 Get2DInputVector(InputState inputState, InputSignal horizontal, InputSignal vertical, bool eightDirectional = false)
    {
        var vector = new Vector2(inputState[horizontal], inputState[vertical]);
        var vectorLength = vector.LengthSquared();
        if (vectorLength > 1)
        {
            vector.Normalize();
        }
        if (eightDirectional && vectorLength > 0)
        {
            var angle = MathF.Atan2(vector.Y, vector.X);
            angle = MathHelper.Snap(angle, MathF.PI / 4);
            vector = MathHelper.AngleToVector(angle);
        }

        return vector;
    }

    private float SpeedThisFrame(bool pressingDash, bool holdingDash)
    {
        if (_slowFrames > 0)
        {
            _slowFrames--;
            return Speed / 2; //recovering from dash
        }

        if (pressingDash)
        {
            if (!holdingDash)
            {
                _slowFrames = DashCooldown;
                _dashFrame = true;
                return DashDistance; //dashing
            }

            return Speed * 1.75f; //sprinting
        }

        return Speed; //walking
    }

    private void Move(Vector2 moveVector)
    {
        if (moveVector != _prevMove)
        {
            X = (float)Math.Floor(X);
            Y = (float)Math.Floor(Y);
        }
        Position += moveVector;

        _prevMove = moveVector;
    }

    private void UpdateAnimations(float walkSpeed)
    {
        if (_sprite is null)
            return;

        var animationName = walkSpeed > 0 ? "walk " : "stand ";
        _sprite.SetAnimation(animationName + SpriteManager.DirectionString8(Facing));
        _sprite.FrameRatio = walkSpeed / 3;
    }

    private void LeaveDashTrail(Vector2 moveVector)
    {
        for (int i = 0; i < _dashTrail!.Length; i++)
        {
            if (_dashTrail[i] is null)
                continue;

            _dashTrail[i]!.Position = Position - moveVector * (i + 1) / _dashTrail.Length;
            _dashTrail[i]!.Visible = true;
        }

        SpriteManager.YSortChildren(this);
    }

    private void UpdateDashTrail()
    {
        for (int i = 0; i < _dashTrail!.Length; i++)
        {
            if (_dashTrail[i] is null)
                continue;

            _dashTrail[i]!.Visible = _slowFrames >= DashCooldown * (i + 1) / (_dashTrail.Length + 1);
        }
    }

    public override void AddChild(string name, Node child)
    {
        if (name == "sprite" && child is SpriteNode s)
        {
            _sprite = s;
        }
        base.AddChild(name, child);
    }

    public override void RemoveChild(string name)
    {
        if (name == "sprite")
        {
            _sprite = null;
        }
        base.RemoveChild(name);
    }
}