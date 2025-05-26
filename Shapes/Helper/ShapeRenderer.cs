using System.Numerics;
using Raylib_cs;

public static class ShapeRenderer
{
    public static void DrawShape(Shape shape, RenderContext context)
    {
        switch (shape)
        {
            case CircleShape circle:
                DrawCircle(circle, context);
                break;

            case PolygonShape polygon:
                DrawPolygon(polygon, context);
                break;

            default:
                throw new NotSupportedException($"Forme non supportée : {shape.GetType().Name}");
        }
    }

    private static void DrawPolygon(PolygonShape shape, RenderContext context)
    {
        var pts = shape.GetTransformedPoints();

        if (context.Fill)
        {
            // Triangulation si besoin
        }
        else
        {
            for (int i = 0; i < pts.Count; i++)
            {
                var a = pts[i];
                var b = pts[(i + 1) % pts.Count];
                Raylib.DrawLineEx(a, b, context.LineThickness, context.Color);
            }
        }
    }

    private static void DrawCircle(CircleShape shape, RenderContext context)
    {
        var pos = shape.GetGlobalPosition();
        if (context.Fill)
            Raylib.DrawCircleV(pos, shape.Radius, context.Color);
        else
            Raylib.DrawCircleLines((int)pos.X, (int)pos.Y, shape.Radius, context.Color);
    }

    private static Vector2 TransformPoint(Vector2 point, Transform2D transform)
    {
        // Rotation + Scale + Position
        float cos = MathF.Cos(transform.Rotation);
        float sin = MathF.Sin(transform.Rotation);

        var rotated = new Vector2(
            point.X * cos - point.Y * sin,
            point.X * sin + point.Y * cos
        );

        var scaled = rotated * transform.Scale;

        return scaled + transform.Position;
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
