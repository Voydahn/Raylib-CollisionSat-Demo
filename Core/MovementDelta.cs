using System.Numerics;

public readonly struct MovementDelta
{
    public Vector2 Translation { get; }
    public float Rotation { get; }

    public MovementDelta(Vector2 translation, float rotation)
    {
        Translation = translation;
        Rotation = rotation;
    }

    public static MovementDelta None => new(Vector2.Zero, 0f);
}