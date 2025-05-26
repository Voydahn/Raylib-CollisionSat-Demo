using System.Numerics;
using Raylib_cs;

public class PolygonShape : Shape
{
    private readonly List<Vector2> _modelPoints;

    public override Vector2 Size
    {
        get
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (var pt in _modelPoints)
            {
                if (pt.X < minX) minX = pt.X;
                if (pt.X > maxX) maxX = pt.X;
                if (pt.Y < minY) minY = pt.Y;
                if (pt.Y > maxY) maxY = pt.Y;
            }

            return new Vector2(maxX - minX, maxY - minY);
        }
    }

    public PolygonShape(IEnumerable<Vector2> modelPoints, Vector2 position, bool isStatic = false)
        : base(position, isStatic)
    {
        Name = "PolygonShape";
        _modelPoints = modelPoints.ToList();
    }

    public override List<Vector2> GetTransformedPoints()
    {
        var result = new List<Vector2>();
        var global = GetGlobalTransform();

        float cosA = MathF.Cos(global.Rotation);
        float sinA = MathF.Sin(global.Rotation);

        foreach (var pt in _modelPoints)
        {
            // ✅ PAS de décalage par Origin ici !
            Vector2 rotated = new Vector2(
                pt.X * cosA - pt.Y * sinA,
                pt.X * sinA + pt.Y * cosA
            );

            Vector2 scaled = rotated * global.Scale;

            result.Add(scaled + global.Position);
        }

        return result;
    }

    public override void OnDraw()
    {
        base.OnDraw();

        var pts = GetTransformedPoints();
        for (int i = 0; i < pts.Count; i++)
        {
            Vector2 a = pts[i];
            Vector2 b = pts[(i + 1) % pts.Count];
            Raylib.DrawLineV(a, b, IsColliding ? Color.Red : Color.Green);
        }
    }
}
