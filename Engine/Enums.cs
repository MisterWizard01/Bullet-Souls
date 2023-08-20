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
        LeftButton,
        MiddleButton,
        RightButton,
        XButton1,
        XButton2,
    }

    public enum MouseAxes
    {
        MouseX,
        MouseY,
        VerticalScroll,
        HorizontalScroll,
    }

    public enum InputSignal
    {
        HorizontalMovement,
        VerticalMovement,
        HorizontalFacing,
        VerticalFacing,
    }

    public enum AnimationEndAction
    {
        Stop,
        Cycle,
        PingPong,
        Reverse,
    }
}