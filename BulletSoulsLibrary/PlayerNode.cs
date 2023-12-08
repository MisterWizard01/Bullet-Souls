using Engine;
using Engine.Managers;
using Engine.Nodes;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using MathHelper = Engine.MathHelper;

namespace BulletSoulsLibrary;

public class PlayerNode : Node2D
{
    public enum PlayerStates
    {
        Main,
        Dash,
        Recover,
        Sprint,
        Slide,
        Parry
    }

    private Vector2 _prevMove;
    private InputState _prevState;
    private int _slowFrames, _parryFrames, _sprintFrames;
    private Vector2 _slideVector;
    private PlayerStates _state;
    private SpriteNode? _sprite;
    private readonly SpriteNode?[] _dashTrail;

    public float Speed { get; set; }
    public float SprintBaseSpeed { get; set; }
    public float SprintAcceleration { get; set; }
    public Vector2 Facing { get; set; }
    public float WeaponCharge { get; set; }
    public float DashDistance { get; set; }
    public int DashCooldown { get; set; }
    public int ParryWindow { get; set; }

    public PlayerNode() : base()
    {
        Facing = new(-1, 1);
        Facing.Normalize();
        _prevState = new();
        _sprite = GetChild("sprite") as SpriteNode;
        _dashTrail = new SpriteNode[4];
    }

    public override void Initialize()
    {
        for (int i = 0; i < 4; i++)
        {
            _dashTrail[i] = GetChild("dashTrail" + i) as SpriteNode;
        }
    }

    public override void Update(Node parent, int frameNumber, InputState inputState)
    {
        var pressingDash = inputState[InputSignal.Dash] > 0;
        var holdingDash = _prevState[InputSignal.Dash] > 0;
        var moveInputVector = Get2DInputVector(inputState, InputSignal.HorizontalMovement, InputSignal.VerticalMovement, true);
        var facingVector = Get2DInputVector(inputState, InputSignal.HorizontalFacing, InputSignal.VerticalFacing, true);

        UpdateState(moveInputVector, pressingDash, holdingDash);
        var moveVector = moveInputVector * SpeedThisFrame();
        if (_state == PlayerStates.Slide)
        {
            moveVector += _slideVector;
        }
        Move(moveVector);

        //Update Facing
        if (_state == PlayerStates.Sprint
            || facingVector.LengthSquared() == 0
            && inputState[InputSignal.Strafe] == 0)
        {
            facingVector = moveVector;
        }
        if (facingVector.LengthSquared() > 0)
        {
            Facing = Vector2.Normalize(facingVector);
        }

        UpdateAnimations(moveVector.Length());
        UpdateTrail(moveVector);

        _prevState = inputState;
        base.Update(this, frameNumber, inputState);
    }

    public override void Draw(Node parent, Camera camera, Vector2 referencePoint)
    {
        base.Draw(this, camera, referencePoint);

        //TODO: maybe draw an input display for debugging?
        //TODO: draw hud
    }

    public void UpdateState(Vector2 moveInputVector, bool pressingDash, bool holdingDash)
    {
        switch (_state)
        {
            case PlayerStates.Main:
                if (!pressingDash || holdingDash)
                    break;
                if (moveInputVector.LengthSquared() == 0)
                {
                    _state = PlayerStates.Parry;
                    _parryFrames = ParryWindow;
                    break;
                }
                _state = PlayerStates.Dash;
                break;

            case PlayerStates.Dash:
                _state = PlayerStates.Recover;
                _slowFrames = DashCooldown;
                break;

            case PlayerStates.Recover:
                _slowFrames--;
                if (_slowFrames > 0)
                    break;
                LeaveDashTrail(Vector2.Zero); //reset all trail sprites back to 0,0
                _state = pressingDash && holdingDash ? PlayerStates.Sprint : PlayerStates.Main;
                break;

            case PlayerStates.Sprint:
                _sprintFrames++;
                if (pressingDash && holdingDash && moveInputVector.LengthSquared() > 0)
                    break;
                _sprintFrames = 0;
                _state = PlayerStates.Slide;
                _slideVector = _prevMove;
                break;

            case PlayerStates.Slide:
                if (_slideVector.Length() > 0.01)
                    break;
                _slideVector = Vector2.Zero;
                _state = PlayerStates.Main;
                break;

            case PlayerStates.Parry:
                _parryFrames--;
                if (_parryFrames > 0)
                    break;
                _state = PlayerStates.Main;
                break;
        }
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
            var angle = MathHelper.VectorToAngle(vector);
            angle = MathHelper.Snap(angle, MathF.PI / 4);
            vector = MathHelper.AngleToVector(angle);
        }

        return vector;
    }

    private float SpeedThisFrame()
    {
        return _state switch
        {
            PlayerStates.Main => Speed,
            PlayerStates.Dash => DashDistance,
            PlayerStates.Recover => Speed / 2,
            PlayerStates.Sprint => SprintBaseSpeed + _sprintFrames * SprintAcceleration,
            PlayerStates.Parry => 0,
            _ => Speed,
        };
    }

    private void Move(Vector2 moveVector)
    {
        var angleDiff = MathHelper.AngleDifference(MathHelper.VectorToAngle(moveVector), MathHelper.VectorToAngle(_prevMove));
        if (_prevMove.LengthSquared() > 0
            && angleDiff > MathF.PI / 8)
        {
            X = (float)Math.Floor(X);
            Y = (float)Math.Floor(Y);
        }
        Position += moveVector;

        _slideVector /= 1.1f;
        _prevMove = moveVector;
    }

    private void UpdateAnimations(float walkSpeed)
    {
        if (_sprite is null)
            return;

        var animationName = "stand ";
        if (walkSpeed > 0)
        {
            animationName = "walk ";
        }
        if (_state == PlayerStates.Dash || _state == PlayerStates.Sprint)
        {
            animationName = "dash ";
        }
        _sprite.SetAnimation(animationName + JuicyContentManager.DirectionString8(Facing));
        _sprite.FrameRatio = walkSpeed / 3;
        
        if (_state != PlayerStates.Parry)
        {
            _sprite.BlendColor = Color.Transparent;
        }
    }

    private void UpdateTrail(Vector2 moveVector)
    {
        var trailCooldown = 0f;
        switch (_state)
        {
            case PlayerStates.Main:

                break;
            case PlayerStates.Dash:
                LeaveDashTrail(moveVector);
                trailCooldown = 1;
                break;
            case PlayerStates.Recover:
                trailCooldown = (float)_slowFrames / DashCooldown;
                break;
            case PlayerStates.Sprint:
                LeaveSprintTrail(moveVector);
                trailCooldown = 1;
                break;
            case PlayerStates.Parry:
                var parryColorIndex = _dashTrail.Length * _parryFrames / (ParryWindow + 1);
                if (_sprite is null)
                    break;
                _sprite.BlendColor = _dashTrail[parryColorIndex]?.BlendColor ?? Color.Transparent;
                break;
        }

        for (int i = 0; i < _dashTrail!.Length; i++)
        {
            if (_dashTrail[i] is null)
                continue;

            _dashTrail[i]!.Visible = trailCooldown >= (float)(i + 1) / (_dashTrail.Length + 1);
        }
    }

    private void LeaveDashTrail(Vector2 moveVector)
    {
        for (int i = 0; i < _dashTrail!.Length; i++)
        {
            if (_dashTrail[i] is null)
                continue;

            _dashTrail[i]!.Position = -moveVector * (i + 1) / _dashTrail.Length;
            _dashTrail[i]!.Visible = true;
            var animationName = moveVector.LengthSquared() > 0 ? "dash " : "stand ";
            _dashTrail[i]!.SetAnimation(animationName + JuicyContentManager.DirectionString8(moveVector));
        }

        JuicyContentManager.YSortChildren(this);
    }

    private void LeaveSprintTrail(Vector2 moveVector)
    {
        for (int i = _dashTrail.Length - 1; i > 0; i--)
        {
            if (_dashTrail[i] is null || _dashTrail[i - 1] is null)
                continue;
            _dashTrail[i]!.Position = _dashTrail[i - 1]!.Position - moveVector;
            _dashTrail[i]!.SetAnimation(_dashTrail[i - 1]!.AnimationName);
        }

        if (_dashTrail[0] is null)
            return;
        _dashTrail[0]!.Position = -4 * moveVector / _dashTrail.Length;
        _dashTrail[0]!.SetAnimation(_sprite?.AnimationName ?? "");
        JuicyContentManager.YSortChildren(this);
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