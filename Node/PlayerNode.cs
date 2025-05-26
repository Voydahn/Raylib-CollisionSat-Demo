using System.Numerics;
using Raylib_cs;

public sealed class PlayerNode : Node
{
    public PolygonShape CollisionBox;
    public SpriteNode SpriteNode { get; set; }

    public bool IsOnGround = false;

    public PlayerNode()
    {
        Name = "PlayerNode";
        IsOnGround = false;
    }

    public void InitializeCollisionBox()
    {
        CollisionBox = ShapeFactory.CreateBox(Vector2.Zero, 40, 40); // Créée locale à (0,0)
        CollisionBox.Owner = this;
        CollisionBox.Position = Position;
        AddChild(CollisionBox);
    }

    public bool CanJump() => IsOnGround;

    public void Jump(float force)
    {
        Velocity = new Vector2(Velocity.X, force);
        IsOnGround = false;
    }

    public override void OnSweepCollision(Vector2 collisionPoint, Vector2 collisionNormal, float deltaTime)
    {
        Position = collisionPoint;
        Vector2 vel = Velocity;
        float vn = Vector2.Dot(vel, collisionNormal);
        if (vn < 0f)
            Velocity -= collisionNormal * vn;
    }
}
