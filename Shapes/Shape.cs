using System.Numerics;
using System.Runtime.InteropServices.Marshalling;
using Raylib_cs;

public abstract class Shape : Node
{
    public Action<Vector2>? CollisionResponse { get; set; } = null;

    public object? Owner { get; set; }
    public override Vector2 Size => new Vector2(40, 40);

    public bool IsStatic { get; set; }
    public bool IsColliding { get; set; }

    protected Shape(Vector2 position, bool isStatic = false)
    {
        Name = "Shape";
        Position = position;
        IsStatic = isStatic;
    }

    public virtual void ApplyMovement(MovementDelta delta)
    {
        if (!IsStatic)
        {
            Position += delta.Translation;
            Rotation += delta.Rotation;
        }
    }

    public abstract List<Vector2> GetTransformedPoints();

    public override void OnDraw()
    {
        base.OnDraw();
        ShapeRenderer.DrawShape(this, new RenderContext(Color.Red));
    }

    public virtual void OnCollisionResponse(Vector2 collisionNormal) { }
}
