using Microsoft.Xna.Framework;

namespace Engine;

public static class MathHelper
{
    /// <summary>
    /// Return 'n' rounded to the nearest multiple of 'grid'.
    /// </summary>
    public static float Snap(float n, float grid)
    {
        return grid * MathF.Round(n / grid);
    }

    /// <summary>
    /// Returns the vector forming the given angle with the x-axis.
    /// </summary>
    /// <returns>A Vector2 with the given length.</returns>
    public static Vector2 AngleToVector(float angle, float length = 1)
    {
        return new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * length;
    }

    /// <summary>
    /// Returns the angle formed between the given vector and the x-axis in radians counterclockwise from {1, 0}.
    /// </summary>
    /// <returns>A float between -pi and pi.</returns>
    public static float VectorToAngle(Vector2 vector)
    {
        return MathF.Atan2(vector.Y, vector.X);
    }

    /// <summary>
    /// Returns the shorter distance between two angles measured in radians.
    /// </summary>
    /// <returns>A float between 0 and pi.</returns>
    public static float AngleDifference(float a, float b)
    {
        float diff = MathF.Abs(a - b);
        if (diff > MathF.PI)
            return MathF.Tau - diff;
        return diff;
    }

    /// <summary>
    /// Returns an angle equivalent the given angle that is between -pi and pi.
    /// </summary>
    /// <returns>A float between -pi and pi.</returns>
    public static float WrapAngle(float angle)
    {
        while (angle < -MathF.PI)
        {
            angle += MathF.Tau;
        }
        while (angle > MathF.PI)
        {
            angle -= MathF.Tau;
        }
        return angle;
    }
}
