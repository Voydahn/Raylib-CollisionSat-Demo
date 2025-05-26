using System.Numerics;

public sealed class BallNode : CircleShape
{
    public BallNode(Vector2 position, float radius, bool isStatic = false)
        : base(position, radius, isStatic)
    {
    }

    public override void OnSweepCollision(Vector2 collisionPoint, Vector2 collisionNormal, float deltaTime)
    {
        Position = collisionPoint;
        Velocity = ReflectWithBoost(Velocity, collisionNormal);
        Velocity = ClampBallAngle(Velocity);
        ClampBallSpeed(this);
    }

    public override void OnCollision(Node other, Vector2 collisionNormal)
    {
        Velocity = ReflectWithBoost(Velocity, collisionNormal);
        Velocity = ClampBallAngle(Velocity);
        ClampBallSpeed(this);
    }

    private Vector2 ReflectWithBoost(Vector2 velocity, Vector2 normal, float boostFactor = 1.05f)
    {
        var reflected = velocity - 2 * Vector2.Dot(velocity, normal) * normal;
        return reflected * boostFactor;
    }

    private void ClampBallSpeed(Node ball, float maxSpeed = 2000f)
    {
        if (ball.Velocity.Length() > maxSpeed)
            ball.Velocity = Vector2.Normalize(ball.Velocity) * maxSpeed;
    }

    private Vector2 ClampBallAngle(Vector2 velocity, float minAngleDegrees = 15f)
    {
        float angle = MathF.Atan2(velocity.Y, velocity.X);

        float absAngle = MathF.Abs(angle);

        float minAngle = MathHelper.ToRadians(minAngleDegrees);

        if (absAngle < minAngle || absAngle > (MathF.PI - minAngle))
        {
            float signX = MathF.Sign(velocity.X);
            float signY = MathF.Sign(velocity.Y);

            float newAngle = minAngle;
            if (absAngle > (MathF.PI / 2)) // entre 90° et 180°
                newAngle = MathF.PI - minAngle;

            float speed = velocity.Length();
            return new Vector2(MathF.Cos(newAngle) * signX, MathF.Sin(newAngle) * signY) * speed;
        }

        return velocity;
    }
}