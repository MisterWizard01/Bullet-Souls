using Engine.Managers;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Engine.Nodes;

public class PlayerNode : PositionableNode
{
    public enum PlayerStates
    {
        main,
        dash,
        recover,
        sprint,
        //slide,
        parry
    }

    private Vector2 _prevMove;
    private InputState _prevState;
    private int _slowFrames, _parryFrames;
    //private Vector2 _slideVector;
    private PlayerStates _state;
    private SpriteNode? _sprite;
    private SpriteNode?[] _dashTrail;

    public float Speed { get; set; }
    public float SprintMultiplier { get; set; }
    public Vector2 Facing { get; set; }
    public float WeaponCharge { get; set; }
    public float DashDistance { get; set; }
    public int DashCooldown { get; set; }
    public int ParryWindow { get; set; }

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
        var moveInputVector = Get2DInputVector(inputState, InputSignal.HorizontalMovement, InputSignal.VerticalMovement, true);
        var facingVector = Get2DInputVector(inputState, InputSignal.HorizontalFacing, InputSignal.VerticalFacing, true);

        UpdateState(moveInputVector, pressingDash, holdingDash);
        //var moveVector = _state == PlayerStates.slide ? _slideVector : moveInputVector * SpeedThisFrame();
        var moveVector = moveInputVector * SpeedThisFrame();
        //Debug.WriteLine(_slideVector);
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
        if (_state == PlayerStates.dash || _state == PlayerStates.sprint)
        {
            LeaveDashTrail(moveVector);
        }
        var trailFade = _state == PlayerStates.parry ? (float)_parryFrames / ParryWindow : (float)_slowFrames / DashCooldown;
        UpdateDashTrail(trailFade);

        _prevState = inputState;
        base.Update(this, frameNumber, inputState);
    }

    public override void Draw(Node parent, Camera camera)
    {
        base.Draw(this, camera);

        //TODO: maybe draw an input display for debugging?
        //TODO: draw hud
    }

    public void UpdateState(Vector2 moveInputVector, bool pressingDash, bool holdingDash)
    {
        switch (_state)
        {
            case PlayerStates.main:
                if (!pressingDash || holdingDash)
                    break;
                if (moveInputVector.LengthSquared() == 0)
                {
                    _state = PlayerStates.parry;
                    _parryFrames = ParryWindow;
                    break;
                }
                _state = PlayerStates.dash;
                break;
            case PlayerStates.dash:
                _state = PlayerStates.recover;
                _slowFrames = DashCooldown;
                break;
            case PlayerStates.recover:
                _slowFrames -= 1;
                if (_slowFrames > 0)
                    break;
                _state = pressingDash && holdingDash ? PlayerStates.sprint : PlayerStates.main;
                break;
            case PlayerStates.sprint:
                if (pressingDash && holdingDash && moveInputVector.LengthSquared() > 0)
                    break;
                _state = PlayerStates.main;
                //_state = PlayerStates.slide;
                //_slideVector = _prevMove;
                break;
            //case PlayerStates.slide:
            //    if (_slideVector.Length() > 0.01)
            //        break;
            //    _state = PlayerStates.main;
            //    break;
            case PlayerStates.parry:
                _parryFrames -= 1;
                Debug.WriteLine("Parrying");
                if (_parryFrames > 0)
                    break;
                _state = PlayerStates.main;
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
            var angle = MathF.Atan2(vector.Y, vector.X);
            angle = MathHelper.Snap(angle, MathF.PI / 4);
            vector = MathHelper.AngleToVector(angle);
        }

        return vector;
    }

    private float SpeedThisFrame()
    {
        return _state switch
        {
            PlayerStates.main => Speed,
            PlayerStates.dash => DashDistance,
            PlayerStates.recover => Speed / 2,
            PlayerStates.sprint => Speed * SprintMultiplier,
            PlayerStates.parry => 0,
            _ => Speed,
        };
    }

    private void Move(Vector2 moveVector)
    {
        if (moveVector != _prevMove)
        {
            X = (float)Math.Floor(X);
            Y = (float)Math.Floor(Y);
        }
        Position += moveVector;

        //_slideVector /= 1.5f;
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
        if (_state == PlayerStates.dash)
        {
            animationName = "dash ";
        }
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
            var animationName = moveVector.LengthSquared() > 0 ? "dash " : "stand ";
            _dashTrail[i]!.SetAnimation(animationName + SpriteManager.DirectionString8(moveVector));
        }

        SpriteManager.YSortChildren(this);
    }

    private void UpdateDashTrail(float percentFade)
    {
        for (int i = 0; i < _dashTrail!.Length; i++)
        {
            if (_dashTrail[i] is null)
                continue;

            _dashTrail[i]!.Visible = percentFade >= (float)(i + 1) / (_dashTrail.Length + 1);
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