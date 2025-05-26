using System.Numerics;
using Raylib_cs;

public class CircleShape : Shape
{
    public float Radius { get; }

    public CircleShape(Vector2 position, float radius, bool isStatic = false)
        : base(position, isStatic)
    {
        Name = "CircleShape";
        Radius = radius;
    }

    public override List<Vector2> GetTransformedPoints()
    {
        const int segments = 20;
        float angleStep = MathF.Tau / segments;

        var global = GetGlobalTransform();
        var points = new List<Vector2>(segments);
        for (int i = 0; i < segments; i++)
        {
            float angle = i * angleStep + global.Rotation;
            points.Add(new Vector2(
                global.Position.X + Radius * MathF.Cos(angle),
                global.Position.Y + Radius * MathF.Sin(angle)
            ));
        }

        return points;
    }

    public override void OnDraw()
    {
        base.OnDraw();

        var col = IsColliding ? Color.Red : Color.Green;

        Raylib.DrawCircleV(Position, Radius, col);
        Raylib.DrawCircleV(Position, 4, Color.Black);
    }
}
