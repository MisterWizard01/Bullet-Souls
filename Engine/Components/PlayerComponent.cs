using Engine.Managers;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Engine.Components;

public class PlayerComponent : IComponent
{
    private Vector2 _prevMove;
    private InputState _prevState;
    private int _slowFrames;

    public float Speed { get; set; }
    public Vector2 Facing { get; set; }
    public float WeaponCharge { get; set; }
    public float DashDistance { get; set; }
    public int DashCooldown { get; set; }

    public PlayerComponent(float speed, float dashDistance)
    {
        Speed = speed;
        DashDistance = dashDistance;
        Facing = new(-1, 1);
        Facing.Normalize();
        _prevState = new();
    }

    public void Update(GameObject gameObject, int frameNumber, InputState inputState)
    {
        if (gameObject == null)
            return;

        var moveVector = SpeedThisFrame(inputState) * Get2DInputVector(inputState, InputSignal.HorizontalMovement, InputSignal.VerticalMovement, true);
        var facingVector = Get2DInputVector(inputState, InputSignal.HorizontalFacing, InputSignal.VerticalFacing, true);

        MoveGameObject(gameObject, moveVector);

        //Update Facing
        if (facingVector.LengthSquared() == 0 && inputState[InputSignal.Strafe] == 0)
        {
            facingVector = moveVector;
        }
        if (facingVector.LengthSquared() > 0)
        {
            Facing = Vector2.Normalize(facingVector);
        }

        UpdateSprite(gameObject, moveVector.Length());

        _prevState = inputState;
    }

    public void Draw(GameObject gameObject, Camera camera)
    {
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

    private float SpeedThisFrame(InputState inputState)
    {
        if (_slowFrames > 0)
        {
            _slowFrames--;
            return Speed / 2; //recovering from dash
        }

        if (inputState[InputSignal.Dash] > 0)
        {
            if (_prevState[InputSignal.Dash] == 0)
            {
                _slowFrames = DashCooldown;
                return DashDistance; //dashing
            }

            return Speed * 1.75f; //sprinting
        }

        return Speed; //walking
    }

    private void MoveGameObject(GameObject gameObject, Vector2 moveVector)
    {
        if (moveVector != _prevMove)
        {
            gameObject.X = (float)Math.Floor(gameObject.X);
            gameObject.Y = (float)Math.Floor(gameObject.Y);
        }
        gameObject.Position += moveVector;

        _prevMove = moveVector;
    }

    private void UpdateSprite(GameObject gameObject, float walkSpeed)
    {
        if (!gameObject.Components.ContainsKey("sprite"))
            return;
        if (gameObject.Components["sprite"] is not SpriteComponent sprite)
            return;

        var animationName = walkSpeed > 0 ? "walk " : "stand ";
        sprite.SetAnimation(animationName + SpriteManager.DirectionString8(Facing));
        sprite.FrameRatio = walkSpeed / 3;
    }
}
