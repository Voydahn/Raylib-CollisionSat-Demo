using System.Numerics;

public static class ShapeFactory
{
    public static PolygonShape CreateTriangle(Vector2 position, float size = 40f, bool isStatic = false)
    {
        var points = new[]
        {
            new Vector2(0, -size),
            new Vector2(size * 0.87f, size * 0.5f),
            new Vector2(-size * 0.87f, size * 0.5f)
        };

        return new PolygonShape(points, position, isStatic);
    }

    public static PolygonShape CreateSquare(Vector2 position, float size = 40f, bool isStatic = false)
    {
        return CreateRegularPolygon(position, 4, size, isStatic);
    }

    public static PolygonShape CreatePentagon(Vector2 position, float size = 40f, bool isStatic = false)
    {
        return CreateRegularPolygon(position, 5, size, isStatic);
    }

    public static PolygonShape CreateBox(Vector2 position, float width, float height, bool isStatic = false)
    {
        var points = new[]
        {
            new Vector2(-width / 2, -height / 2),
            new Vector2(width / 2, -height / 2),
            new Vector2(width / 2, height / 2),
            new Vector2(-width / 2, height / 2)
        };

        return new PolygonShape(points, position, isStatic);
    }

    public static CircleShape CreateCircle(Vector2 position, float radius, bool isStatic = false)
    {
        return new CircleShape(position, radius, isStatic);
    }

    public static BallNode CreateBall(Vector2 position, float radius, bool isStatic = false)
    {
        return new BallNode(position, radius, isStatic);
    }

    public static PolygonShape CreateRegularPolygon(Vector2 position, int sides, float radius, bool isStatic = false)
    {
        if (sides < 3)
            throw new ArgumentException("Un polygone régulier doit avoir au moins 3 côtés.", nameof(sides));

        var points = new List<Vector2>();
        for (int i = 0; i < sides; i++)
        {
            float angle = i * MathF.Tau / sides;
            points.Add(new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius);
        }

        return new PolygonShape(points, position, isStatic);
    }
}