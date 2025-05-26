using System.Numerics;
using Raylib_cs;

public class RenderShape
{
    public static void DrawShape(object shape)
    {
        if (shape is PolygonShape p)
        {
            DrawPolygon(p, p.IsColliding ? Color.Red : Color.Green);
        }
        else if (shape is CircleShape c)
        {
            var col = c.IsColliding ? Color.Red : Color.Green;
            Raylib.DrawCircleV(c.Position, c.Radius, col);
            Raylib.DrawCircleV(c.Position, 4, Color.Black);
        }
    }

    public static void DrawPolygon(PolygonShape poly, Color color)
    {
        var pts = poly.GetTransformedPoints();
        for (int i = 0; i < pts.Count; i++)
        {
            Vector2 a = pts[i];
            Vector2 b = pts[(i + 1) % pts.Count];
            Raylib.DrawLineV(a, b, color);
        }

        Raylib.DrawCircleV(poly.Position, 4, Color.Black);
    }
}