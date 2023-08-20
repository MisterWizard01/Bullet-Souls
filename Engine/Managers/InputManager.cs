using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Managers;

///Polls the keyboard, mouse, and gamepad each frame, then reports a list of signals (floats) representing active inputs.
public class InputManager
{
    public Dictionary<InputMode, IInput[]> Bindings;
    public InputState InputState { get; set; }
    public InputMode Mode { get; set; }

    public InputManager(InputMode mode)
    {
        Mode = mode;
        InputState = new InputState();
        Bindings = new Dictionary<InputMode, IInput[]>()
        {
            { InputMode.mouseAndKeyboard, new IInput[InputState.SignalCount] },
            { InputMode.keyboardOnly, new IInput[InputState.SignalCount] },
            { InputMode.XBoxController, new IInput[InputState.SignalCount] },
        };
    }

    public void Update()
    {
        float[] signals = new float[InputState.SignalCount];
        for (int i = 0; i < signals.Length; i++)
        {
            signals[i] = Bindings[Mode][i]?.GetSignalValue(Mouse.GetState(), Keyboard.GetState(), GamePad.GetState(1)) ?? 0;
        }

        InputState = new InputState(signals);
    }

    public void SetBinding(InputSignal signalName, IInput input)
    {
        SetBinding(Mode, signalName, input);
    }

    public void SetBinding(InputMode mode, InputSignal signalName, IInput input)
    {
        Bindings[mode][(int)signalName] = input;
    }
}
