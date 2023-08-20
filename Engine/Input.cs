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
    public float GetSignalValue(MouseState mouseState, KeyboardState keyboardState, GamePadState gamePadState, float reference = 0);
}

public class KeyInput : IInput
{
    public Keys negativeKey, positiveKey;

    public KeyInput(Keys negativeKey, Keys positiveKey)
    {
        this.negativeKey = negativeKey;
        this.positiveKey = positiveKey;
    }

    public float GetSignalValue(MouseState mouseState, KeyboardState keyboardState, GamePadState gamePadState, float reference = 0)
    {
        return (keyboardState.IsKeyDown(positiveKey) ? 1 : 0) - (keyboardState.IsKeyDown(negativeKey) ? 1 : 0);
    }
}

public class MouseButtonInput : IInput
{
    public MouseButtons negativeButton, positiveButton;

    public float GetSignalValue(MouseState mouseState, KeyboardState keyboardState, GamePadState gamePadState, float reference = 0)
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

    public float GetSignalValue(MouseState mouseState, KeyboardState keyboardState, GamePadState gamePadState, float reference = 0)
    {
        return mouseAxis switch
        {
            MouseAxes.MouseX => mouseState.X - reference,
            MouseAxes.MouseY => mouseState.Y - reference,
            MouseAxes.VerticalScroll => mouseState.ScrollWheelValue - reference,
            MouseAxes.HorizontalScroll => mouseState.HorizontalScrollWheelValue - reference,
            _ => 0,
        };
    }
}

public class GamePadButtonInput : IInput
{
    public Buttons negativeButton, positiveButton;

    public float GetSignalValue(MouseState mouseState, KeyboardState keyboardState, GamePadState gamePadState, float reference = 0)
    {
        return (gamePadState.IsButtonDown(positiveButton) ? 1 : 0) - (gamePadState.IsButtonDown(negativeButton) ? 1 : 0);
    }
}

public class GamePadAxisInput : IInput
{
    public Buttons gamePadAxis;

    public float GetSignalValue(MouseState mouseState, KeyboardState keyboardState, GamePadState gamePadState, float reference = 0)
    {
        throw new NotImplementedException();
    }
}