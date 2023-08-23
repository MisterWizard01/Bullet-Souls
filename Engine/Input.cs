using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Engine;

public class Binding
{
    public InputSignal SignalName { get; set; }
    public Dictionary<InputMode, IInput> Inputs { get; set; }

    public Binding(InputSignal signalName, InputMode mode, IInput input)
    {
        SignalName = signalName;
        Inputs = new Dictionary<InputMode, IInput>
        {
            { mode, input }
        };
    }
}

public interface IInput
{
    public float GetSignalValue(MouseState mouseState, KeyboardState keyboardState, GamePadState gamePadState, Vector2 referencePoint);
    public float GetSignalValue(MouseState mouseState, KeyboardState keyboardState, GamePadState gamePadState)
    {
        return GetSignalValue(mouseState, keyboardState, gamePadState, Vector2.Zero);
    }
}

public class KeyInput : IInput
{
    public Keys negativeKey, positiveKey;

    public KeyInput(Keys negativeKey, Keys positiveKey)
    {
        this.negativeKey = negativeKey;
        this.positiveKey = positiveKey;
    }

    public float GetSignalValue(MouseState mouseState, KeyboardState keyboardState, GamePadState gamePadState, Vector2 referencePoint)
    {
        return (keyboardState.IsKeyDown(positiveKey) ? 1 : 0) - (keyboardState.IsKeyDown(negativeKey) ? 1 : 0);
    }
}

public class MouseButtonInput : IInput
{
    public MouseButtons negativeButton, positiveButton;

    public float GetSignalValue(MouseState mouseState, KeyboardState keyboardState, GamePadState gamePadState, Vector2 referencePoint)
    {
        return ButtonToFloat(mouseState, positiveButton) - ButtonToFloat(mouseState, negativeButton);
    }

    public static float ButtonToFloat(MouseState mouseState, MouseButtons mouseControl)
    {
        return mouseControl switch
        {
            MouseButtons.LeftButton => mouseState.LeftButton == ButtonState.Pressed ? 1 : 0,
            MouseButtons.MiddleButton => mouseState.MiddleButton == ButtonState.Pressed ? 1 : 0,
            MouseButtons.RightButton => mouseState.RightButton == ButtonState.Pressed ? 1 : 0,
            MouseButtons.XButton1 => mouseState.XButton1 == ButtonState.Pressed ? 1 : 0,
            MouseButtons.XButton2 => mouseState.XButton2 == ButtonState.Pressed ? 1 : 0,
            _ => (float)0,
        };
    }
}

public class MouseAxisInput : IInput
{
    public MouseAxes mouseAxis;
    public bool inverted;

    public MouseAxisInput(MouseAxes mouseAxis, bool inverted = false)
    {
        this.mouseAxis = mouseAxis;
        this.inverted = inverted;
    }

    public float GetSignalValue(MouseState mouseState, KeyboardState keyboardState, GamePadState gamePadState, Vector2 referencePoint)
    {
        var direction = Vector2.Normalize(mouseState.Position.ToVector2() - referencePoint);
        return (inverted ? -1 : 1) * mouseAxis switch
        {
            MouseAxes.MouseX => mouseState.X - referencePoint.X,
            MouseAxes.MouseY => mouseState.Y - referencePoint.Y,
            MouseAxes.VerticalScroll => mouseState.ScrollWheelValue - referencePoint.X,
            MouseAxes.HorizontalScroll => mouseState.HorizontalScrollWheelValue - referencePoint.Y,
            _ => 0,
        };
    }
}

public class GamePadButtonInput : IInput
{
    public Buttons negativeButton, positiveButton;

    public GamePadButtonInput(Buttons negativeButton, Buttons positiveButton)
    {
        this.negativeButton = negativeButton;
        this.positiveButton = positiveButton;
    }

    public float GetSignalValue(MouseState mouseState, KeyboardState keyboardState, GamePadState gamePadState, Vector2 referencePoint)
    {
        return (gamePadState.IsButtonDown(positiveButton) ? 1 : 0) - (gamePadState.IsButtonDown(negativeButton) ? 1 : 0);
    }
}

public class GamePadAxisInput : IInput
{
    public GamePadAxes gamePadAxis;
    public bool inverted;

    public GamePadAxisInput(GamePadAxes gamePadAxis, bool inverted = false)
    {
        this.gamePadAxis = gamePadAxis;
        this.inverted = inverted;
    }

    public float GetSignalValue(MouseState mouseState, KeyboardState keyboardState, GamePadState gamePadState, Vector2 referencePoint)
    {
        return (inverted ? -1 : 1) * gamePadAxis switch
        {
            GamePadAxes.LeftStickX => gamePadState.ThumbSticks.Left.X,
            GamePadAxes.LeftStickY => gamePadState.ThumbSticks.Left.Y,
            GamePadAxes.RightStickX => gamePadState.ThumbSticks.Right.X,
            GamePadAxes.RightStickY => gamePadState.ThumbSticks.Right.Y,
            GamePadAxes.LeftTrigger => gamePadState.Triggers.Left,
            GamePadAxes.RightTrigger => gamePadState.Triggers.Right,
            _ => 0,
        };
    }
}