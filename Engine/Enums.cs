namespace Engine
{
    public enum InputMode
    {
        mouseAndKeyboard,
        keyboardOnly,
        XBoxController,
    }

    public enum MouseButtons
    {
        None,
        LeftButton,
        MiddleButton,
        RightButton,
        XButton1,
        XButton2,
    }

    public enum MouseAxes
    {
        None,
        MouseX,
        MouseY,
        VerticalScroll,
        HorizontalScroll,
    }

    public enum GamePadAxes
    {
        None,
        LeftStickX,
        LeftStickY,
        RightStickX,
        RightStickY,
        LeftTrigger,
        RightTrigger,
    }

    public enum InputSignal
    {
        HorizontalMovement,
        VerticalMovement,
        HorizontalFacing,
        VerticalFacing,
        Fire,
        Dash,
        Recover,
        Strafe,
        Grenade,
        Interact,
    }

    public enum AnimationEndAction
    {
        Stop,
        Cycle,
        PingPong,
        Reverse,
    }
}