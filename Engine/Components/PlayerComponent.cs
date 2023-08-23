using Engine.Managers;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Engine.Components;

public class PlayerComponent : IComponent
{
    public float Speed { get; set; }
    public Vector2 Facing { get; set; }
    public float WeaponCharge { get; set; }

    public PlayerComponent(float speed)
    {
        Speed = speed;
        Facing = new(-1, 1);
        Facing.Normalize();
    }

    public void Update(GameObject gameObject, int frameNumber, InputState inputState)
    {
        if (gameObject == null)
            return;

        var moveVector = Speed * Get2DInputVector(inputState, InputSignal.HorizontalMovement, InputSignal.VerticalMovement);
        var facingVector = Get2DInputVector(inputState, InputSignal.HorizontalFacing, InputSignal.VerticalFacing);
        
        gameObject.Position += moveVector;

        if (facingVector.LengthSquared() == 0)
        {
            facingVector = moveVector;
        }
        if (facingVector.LengthSquared() > 0)
        {
            Facing = Vector2.Normalize(facingVector);
        }
        UpdateSprite(gameObject, moveVector);
    }

    public void Draw(GameObject gameObject, Camera camera)
    {
        //TODO: maybe draw an input display for debugging?
    }

    public static Vector2 Get2DInputVector(InputState inputState, InputSignal horizontal, InputSignal vertical)
    {
        var vector = new Vector2(
            inputState.GetInput(horizontal),
            inputState.GetInput(vertical)
        );
        var vectorLength = vector.LengthSquared();
        if (vectorLength > 1)
        {
            vector.Normalize();
        }
        return vector;
    }

    private void UpdateSprite(GameObject gameObject, Vector2 movement)
    {
        if (!gameObject.Components.ContainsKey("sprite"))
            return;
        if (gameObject.Components["sprite"] is not SpriteComponent sprite)
            return;

        var walkSpeed = movement.Length();
        var animationName = walkSpeed > 0 ? "walk " : "stand ";
        sprite.SetAnimation(animationName + SpriteManager.DirectionString8(Facing));
        sprite.FrameRatio = walkSpeed / 3;
    }
}
