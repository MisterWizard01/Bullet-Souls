using Microsoft.Xna.Framework;

namespace Engine;

public static class MathHelper
{
    public static float Snap(float n, float grid)
    {
        return grid * MathF.Round(n / grid);
    }

    public static Vector2 AngleToVector(float angle, float length = 1)
    {
        return new(MathF.Cos(angle) * length, MathF.Sin(angle) * length);
    }
}
