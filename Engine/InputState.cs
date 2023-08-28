namespace Engine;

public struct InputState
{
    private float[] signals;
    public readonly int SignalCount;

    public float this[InputSignal signal] => GetInput(signal);
    //public bool this[InputSignal signal] => GetInput(signal) > 0;

    public InputState()
    {
        SignalCount = Enum.GetValues(typeof(InputSignal)).Length;
        signals = new float[SignalCount];
    }

    public InputState(float[] signals)
    {
        SignalCount = Enum.GetValues(typeof(InputSignal)).Length;
        if (signals.Length != SignalCount)
        {
            throw new Exception("Expecting array of size " + SignalCount + " and got " + signals.Length + ".");
        }
        this.signals = signals;
    }

    public float GetInput(InputSignal signal)
    {
        return signals[(int)signal];
    }

    public float[] GetInputs()
    {
        var ret = new float[SignalCount];
        Array.Copy(signals, ret, SignalCount);
        return ret;
    }

    public void SetInput(InputSignal signal, float value)
    {
        signals[(int)signal] = value;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not InputState)
        {
            return false;
        }

        var other = (InputState)obj;
        for (int i = 0; i < SignalCount; i++)
        {
            if (other.signals[i] != signals[i])
            {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(signals);
    }
}