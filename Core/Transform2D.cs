using System.Numerics;

public readonly struct Transform2D
{
    public readonly Vector2 Position;
    public readonly float Rotation; // En radians
    public readonly Vector2 Scale;

    public Transform2D(Vector2 position, float rotation, Vector2 scale)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }

    public static Transform2D Identity => new(Vector2.Zero, 0f, Vector2.One);

    public Transform2D Combine(Transform2D parent)
    {
        var scaled = Position * parent.Scale;
        var rotated = RotateVector(scaled, parent.Rotation);
        var newPos = rotated + parent.Position;
        var newRot = Rotation + parent.Rotation;
        var newScale = Scale * parent.Scale;

        return new Transform2D(newPos, newRot, newScale);
    }

    private static Vector2 RotateVector(Vector2 v, float angle)
    {
        float cos = MathF.Cos(angle);
        float sin = MathF.Sin(angle);

        return new Vector2(
            v.X * cos - v.Y * sin,
            v.X * sin + v.Y * cos
        );
    }
}