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
        Vector2 rotatedVector = new Vector2();
        rotatedVector.x = -(velocity.x * Mathf.Cos(radians) - velocity.y * Mathf.Sin(radians));
        rotatedVector.y = -(velocity.x * Mathf.Sin(radians) + velocity.y * Mathf.Cos(radians));
        
        return rotatedVector;
    }

    public static bool CheckTransformDistance(this Transform a, Transform b, float distanceThreshold)
    {
        return a.position.CheckVector3Distance(b.position, distanceThreshold);
    }

    public static bool CheckVector3Distance(this Vector3 a, Vector3 b, float distanceThreshold)
    {
        var dist = Vector3.Distance(a, b);
        // Debug.Log(dist);
        return dist < distanceThreshold;
    }
}

public enum Axis 
{
    X, Y, Z
}
