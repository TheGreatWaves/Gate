using UnityEngine;

public static class VectorsExtension
{
    public static Vector3 WithAxis(this Vector3 vector, Axis axis, float value)
    {
        return new Vector3(
            axis == Axis.X ? value : vector.x,
            axis == Axis.Y ? value : vector.y,
            axis == Axis.Z ? value : vector.z
        );
    }

    public static Vector2 RotateVector2(this Vector2 velocity, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        float x = velocity.x * cos - velocity.y * sin;
        float y = velocity.x * sin + velocity.y * cos;
        return new Vector2(x, y);
    }
}

public enum Axis 
{
    X, Y, Z
}
