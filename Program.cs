using Raylib_cs;
using System.Numerics;
using CollisionSatSystem.Core;

class Program
{
    static List<Shape> shapes = new();

    static CollisionMode currentMode = CollisionMode.SAT;

    static void Main()
    {
        Raylib.InitWindow(800, 600, "Raylib-cs - Plateforme SAT amélioré");
        Raylib.SetTargetFPS(60);

        var player = new PlayerNode()
        {
            Anchor = AnchorEnum.Center,
            Position = new Vector2(400, 300),
            Scale = Vector2.One
        };

        player.InitializeCollisionBox();
        shapes.Add(player.CollisionBox);

        const float moveSpeed = 200f;
        const float rotSpeed = 2.0f;
        Vector2 gravity = new Vector2(0, 500f);
        float jumpForce = -400f;

        bool isOnGround = false;
        float coyoteTimer = 0f;
        const float coyoteTimeMax = 0.1f;
        const float groundNormalThreshold = 0.7f;

        List<(Vector2 position, Vector2 vector)> debugVectors = new();

        // Préparation des polygones
        var triangle = ShapeFactory.CreateTriangle(new Vector2(200, 400));
        triangle.IsStatic = true;
        shapes.Add(triangle);

        var pentagon = ShapeFactory.CreatePentagon(new Vector2(500, 332), 50);
        pentagon.IsStatic = true;
        shapes.Add(pentagon);

        // Sol et murs
        var ground = ShapeFactory.CreateBox(new Vector2(400, 580), 800, 20);
        ground.IsStatic = true;
        shapes.Add(ground);

        var ceiling = ShapeFactory.CreateBox(new Vector2(400, 0), 800, 20);
        ceiling.IsStatic = true;
        shapes.Add(ceiling);

        var wallLeft = ShapeFactory.CreateBox(new Vector2(0, 300), 20, 600);
        wallLeft.IsStatic = true;
        shapes.Add(wallLeft);

        var wallRight = ShapeFactory.CreateBox(new Vector2(800, 300), 20, 600);
        wallRight.IsStatic = true;
        shapes.Add(wallRight);

        var platform = ShapeFactory.CreateBox(new Vector2(400, 490), 300, 20);
        // platform.Angle = platform.Angle + MathF.PI / 3;
        platform.IsStatic = true;
        shapes.Add(platform);

        var circleObs = ShapeFactory.CreateCircle(new Vector2(600, 300), 30f);
        circleObs.IsStatic = true;
        shapes.Add(circleObs);

        var ball = ShapeFactory.CreateBall(new Vector2(400, 200), 10f);
        ball.Owner = ball;
        ball.Velocity = new Vector2(200, 200);
        shapes.Add(ball);

        while (!Raylib.WindowShouldClose())
        {
            float deltaTime = Raylib.GetFrameTime();
            debugVectors.Clear();

            // ball.Position += ball.Velocity * deltaTime;
            TrySweepMove(ball, ball.Velocity * deltaTime, shapes, deltaTime);

            // Changement de mode de collision
            if (Raylib.IsKeyPressed(KeyboardKey.O)) currentMode = CollisionMode.SAT;
            if (Raylib.IsKeyPressed(KeyboardKey.P)) currentMode = CollisionMode.DIAGS;

            // --- Contrôles du carré (WASD + Q/E + espace)
            Vector2 squareMove = Vector2.Zero;
            float squareRot = 0f;
            if (Raylib.IsKeyDown(KeyboardKey.A)) squareMove.X -= 1;
            if (Raylib.IsKeyDown(KeyboardKey.D)) squareMove.X += 1;
            if (Raylib.IsKeyDown(KeyboardKey.W)) squareMove.Y -= 1;
            if (Raylib.IsKeyDown(KeyboardKey.S)) squareMove.Y += 1;
            if (Raylib.IsKeyDown(KeyboardKey.Q)) squareRot -= 1;
            if (Raylib.IsKeyDown(KeyboardKey.E)) squareRot += 1;

            // Saut
            if (Raylib.IsKeyPressed(KeyboardKey.Space) && isOnGround)
            {
                player.Velocity.Y = jumpForce;
                isOnGround = false;
                coyoteTimer = 0f;
            }

            if (squareMove != Vector2.Zero)
                squareMove = Vector2.Normalize(squareMove) * moveSpeed * deltaTime;

            // Gravité + intégration de la vélocité
            player.Velocity += gravity * deltaTime;

            // player.Position += squareMove + player.Velocity * deltaTime;
            player.Rotation += squareRot * rotSpeed * deltaTime;

            Vector2 playerMove = squareMove + player.Velocity * deltaTime;
            TrySweepMove(player, playerMove, shapes, deltaTime);

            // Réinitialise l’état de collision
            foreach (var s in shapes)
            {
                if (s is PolygonShape p) p.IsColliding = false;
                else if (s is CircleShape c) c.IsColliding = false;
            }

            // AJOUT : collecte des normals de contact pour le joueur
            var contactNormals = new List<Vector2>();

            // --- Tests de collision
            for (int i = 0; i < shapes.Count; i++)
            {
                for (int j = i + 1; j < shapes.Count; j++)
                {
                    var A = shapes[i];
                    var B = shapes[j];
                    bool hit = false;
                    Vector2 pushOut = Vector2.Zero;

                    // CIRCLE vs CIRCLE
                    if (A is CircleShape c1 && B is CircleShape c2)
                    {
                        hit = ResolveCircleCircle(c1, c2, out pushOut);
                    }
                    // CIRCLE vs POLYGON
                    else if (A is CircleShape circ && B is PolygonShape poly)
                    {
                        hit = ResolveSATCirclePolygon(circ, poly, out pushOut);
                    }
                    else if (A is PolygonShape poly2 && B is CircleShape circ2)
                    {
                        hit = ResolveSATCirclePolygon(circ2, poly2, out pushOut);
                        pushOut = -pushOut;
                    }
                    else if (A is PolygonShape polyA && B is PolygonShape polyB)
                    {
                        switch (currentMode)
                        {
                            case CollisionMode.SAT:
                                hit = ResolveSATCollisionForGravity(polyA, polyB, out pushOut);
                                break;
                            case CollisionMode.DIAGS:
                                hit = ResolveCollision_DIAGS(polyA, polyB);
                                break;
                        }
                    }

                    if (!hit) continue;

                    // flag colliding
                    if (A is PolygonShape polyAFlag) polyAFlag.IsColliding = true;
                    else if (A is CircleShape circleA) circleA.IsColliding = true;

                    if (B is PolygonShape polyBFlag) polyBFlag.IsColliding = true;
                    else if (B is CircleShape circleB) circleB.IsColliding = true;

                    // collecte des normals si l’un est le joueur
                    if (A.Owner == player || B.Owner == player)
                    {
                        Vector2 dir = (A.Owner == player) ? -pushOut : pushOut;
                        Vector2 normal = Vector2.Normalize(dir);
                        contactNormals.Add(normal);
                        debugVectors.Add((player.CollisionBox.Position, dir));
                    }

                    if (A.Owner == ball || B.Owner == ball)
                    {
                        Vector2 normal = Vector2.Normalize((A.Owner == ball) ? -pushOut : pushOut);

                        ball.Velocity = ReflectWithBoost(ball.Velocity, normal);
                        ball.Velocity = ClampBallAngle(ball.Velocity);
                        ClampBallSpeed(ball);
                    }
                }
            }


            // AJOUT : détermination de isOnGround via seuil angulaire
            bool isTouchingGround = contactNormals
                .Any(n => n.Y < -groundNormalThreshold);

            bool isTouchingCeiling = contactNormals
                .Any(n => n.Y > groundNormalThreshold);

            if (isTouchingCeiling && player.Velocity.Y < 0f)
                player.Velocity.Y = 0f;

            // AJOUT : gestion du « coyote time »
            if (isTouchingGround)
            {
                isOnGround = true;
                coyoteTimer = coyoteTimeMax;
            }
            else if (coyoteTimer > 0f)
            {
                coyoteTimer -= deltaTime;
                isOnGround = true;
            }
            else
            {
                isOnGround = false;
            }

            // --- AJOUT : stabilisation de la vélocité verticale
            if (isOnGround && player.Velocity.Y > 0)
                player.Velocity.Y = 0f;

            // --- (OPTIONNEL) ground‑snap vers le haut si un léger recouvrement persiste
            // const float snapAmount = 0.5f;
            // if (isOnGround)
            //     square.Position += new Vector2(0, -snapAmount);

            // --- Rendu
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.RayWhite);

            // Affiche debug vectors et état
            foreach (var (pos, vec) in debugVectors)
            {
                Vector2 end = pos + vec * 10f;
                Raylib.DrawLineV(pos, end, Color.DarkBlue);
                Vector2 perp = Vector2.Normalize(new Vector2(-vec.Y, vec.X)) * 5f;
                Raylib.DrawLineV(end, end - vec * 0.2f + perp, Color.DarkBlue);
                Raylib.DrawLineV(end, end - vec * 0.2f - perp, Color.DarkBlue);
            }

            // Texte ON GROUND / IN AIR
            if (isOnGround)
                Raylib.DrawText("ON GROUND", 10, 40, 20, Color.DarkGreen);
            else
                Raylib.DrawText("IN AIR", 10, 40, 20, Color.Red);

            // Affiche mode de collision
            Raylib.DrawText(currentMode.ToString(), 10, 10, 20, Color.DarkGray);

            player.Draw();

            // Dessine tous les polygones
            foreach (var s in shapes)
            {
                s.Draw();
            }

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    static Vector2 ReflectWithBoost(Vector2 velocity, Vector2 normal, float boostFactor = 1.05f)
    {
        var reflected = velocity - 2 * Vector2.Dot(velocity, normal) * normal;
        return reflected * boostFactor;
    }

    static void ClampBallSpeed(Node ball, float maxSpeed = 2000f)
    {
        if (ball.Velocity.Length() > maxSpeed)
            ball.Velocity = Vector2.Normalize(ball.Velocity) * maxSpeed;
    }

    static Vector2 ClampBallAngle(Vector2 velocity, float minAngleDegrees = 15f)
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


    private static bool TrySweepMove(Node mover, Vector2 desiredMove, List<Shape> colliders, float deltaTime)
    {
        Vector2 from = mover.GetGlobalPosition();
        Vector2 to = from + desiredMove;

        const int maxCollisionsPerFrame = 4;
        int collisionsThisFrame = 0;

        while (collisionsThisFrame < maxCollisionsPerFrame)
        {
            bool collisionDetected = false;
            Vector2 collisionPoint = Vector2.Zero;
            Vector2 collisionNormal = Vector2.Zero;

            foreach (var collider in colliders)
            {
                if (collider == mover) continue;
                if (collider is PolygonShape poly)
                {
                    if (SweepTestPolygon(from, to, poly, out collisionPoint, out collisionNormal))
                    {
                        collisionDetected = true;
                        break;
                    }
                }
            }

            if (collisionDetected)
            {
                collisionsThisFrame++;

                mover.Position = collisionPoint;

                if (mover is CircleShape) // donc ball
                {
                    mover.Velocity = ReflectWithBoost(mover.Velocity, collisionNormal);
                    mover.Velocity = ClampBallAngle(mover.Velocity);
                    ClampBallSpeed(mover);

                    from = mover.GetGlobalPosition();
                    float timeLeft = deltaTime * (1f - collisionsThisFrame * 0.2f);
                    to = from + mover.Velocity * timeLeft;
                }
                else // donc player
                {
                    Vector2 vel = mover.Velocity;
                    float vn = Vector2.Dot(vel, collisionNormal);
                    if (vn < 0f)
                        mover.Velocity -= collisionNormal * vn; // enlève juste la composante vers l'obstacle

                    from = mover.GetGlobalPosition();
                    float timeLeft = deltaTime * (1f - collisionsThisFrame * 0.2f);
                    to = from + mover.Velocity * timeLeft;
                }
            }
            else
            {
                mover.Position = to;
                return false;
            }
        }

        return true;
    }

    static bool ResolveSATCollision(PolygonShape a, PolygonShape b)
    {
        var ptsA = a.GetTransformedPoints();
        var ptsB = b.GetTransformedPoints();

        float minOverlap = float.MaxValue;
        Vector2 smallestAxis = Vector2.Zero;

        for (int shape = 0; shape < 2; shape++)
        {
            var poly = (shape == 0) ? ptsA : ptsB;

            for (int i = 0; i < poly.Count; i++)
            {
                int j = (i + 1) % poly.Count;
                Vector2 edge = poly[j] - poly[i];
                Vector2 axis = new Vector2(-edge.Y, edge.X);
                axis = Vector2.Normalize(axis);

                (float minA, float maxA) = ProjectPolygon(axis, ptsA);
                (float minB, float maxB) = ProjectPolygon(axis, ptsB);

                float overlap = MathF.Min(maxA, maxB) - MathF.Max(minA, minB);

                if (overlap <= 0)
                    return false;

                if (overlap < minOverlap)
                {
                    minOverlap = overlap;
                    smallestAxis = axis;
                }
            }
        }

        Vector2 dir = b.GetGlobalPosition() - a.GetGlobalPosition();
        if (Vector2.Dot(dir, smallestAxis) < 0)
            smallestAxis *= -1;

        Vector2 resolution = smallestAxis * minOverlap;

        ApplyPush(a, b, resolution);

        return true;
    }

    static bool ResolveSATCollisionForGravity(PolygonShape a, PolygonShape b, out Vector2 pushOut)
    {
        var ptsA = a.GetTransformedPoints();
        var ptsB = b.GetTransformedPoints();

        float minOverlap = float.MaxValue;
        Vector2 smallestAxis = Vector2.Zero;

        for (int shape = 0; shape < 2; shape++)
        {
            var poly = (shape == 0) ? ptsA : ptsB;

            for (int i = 0; i < poly.Count; i++)
            {
                int j = (i + 1) % poly.Count;
                Vector2 edge = poly[j] - poly[i];
                Vector2 axis = new Vector2(-edge.Y, edge.X);
                axis = Vector2.Normalize(axis);

                (float minA, float maxA) = ProjectPolygon(axis, ptsA);
                (float minB, float maxB) = ProjectPolygon(axis, ptsB);

                float overlap = MathF.Min(maxA, maxB) - MathF.Max(minA, minB);

                if (overlap <= 0)
                {
                    pushOut = Vector2.Zero;
                    return false;
                }

                if (overlap < minOverlap)
                {
                    minOverlap = overlap;
                    smallestAxis = axis;
                }
            }
        }

        Vector2 dir = b.GetGlobalPosition() - a.GetGlobalPosition();
        if (Vector2.Dot(dir, smallestAxis) < 0)
            smallestAxis *= -1;

        Vector2 resolution = smallestAxis * minOverlap;

        ApplyPush(a, b, resolution);

        pushOut = resolution;
        return true;
    }

    static (float min, float max) ProjectPolygon(Vector2 axis, List<Vector2> points)
    {
        float min = Vector2.Dot(axis, points[0]);
        float max = min;
        for (int i = 1; i < points.Count; i++)
        {
            float proj = Vector2.Dot(axis, points[i]);
            if (proj < min) min = proj;
            if (proj > max) max = proj;
        }
        return (min, max);
    }

    static bool ResolveCollision_DIAGS(PolygonShape a, PolygonShape b)
    {
        bool hasIntersection = false;

        var ptsA = a.GetTransformedPoints();
        var ptsB = b.GetTransformedPoints();

        // Diagonales de A
        foreach (var pt in ptsA)
        {
            Vector2 from = a.Position;
            Vector2 to = pt;
            Vector2 direction = to - from;

            for (int i = 0; i < ptsB.Count; i++)
            {
                Vector2 b1 = ptsB[i];
                Vector2 b2 = ptsB[(i + 1) % ptsB.Count];

                if (LinesIntersect(from, to, b1, b2, out float t1, out _))
                {
                    Vector2 push = (1.0f - t1) * direction;
                    a.Position -= push;
                    hasIntersection = true;
                }
            }
        }

        // Diagonales de B
        foreach (var pt in ptsB)
        {
            Vector2 from = b.Position;
            Vector2 to = pt;
            Vector2 direction = to - from;

            for (int i = 0; i < ptsA.Count; i++)
            {
                Vector2 a1 = ptsA[i];
                Vector2 a2 = ptsA[(i + 1) % ptsA.Count];

                if (LinesIntersect(from, to, a1, a2, out float t1, out _))
                {
                    Vector2 push = (1.0f - t1) * direction;
                    b.Position -= push;
                    hasIntersection = true;
                }
            }
        }

        return hasIntersection;
    }

    // Copie fidèle de la logique d'intersection pondérée (comme t1 en C++)
    static Vector2? GetDisplacementAlongDiagonal(Vector2 from, Vector2 to, List<Vector2> polygon)
    {
        Vector2 displacement = Vector2.Zero;
        Vector2 direction = to - from;

        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 p1 = polygon[i];
            Vector2 p2 = polygon[(i + 1) % polygon.Count];

            if (LinesIntersect(from, to, p1, p2, out float t1, out _))
            {
                displacement += (1.0f - t1) * direction;
            }
        }

        return displacement == Vector2.Zero ? null : displacement;
    }

    static bool LinesIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out float t1, out float t2)
    {
        float d = (a2.X - a1.X) * (b2.Y - b1.Y) - (a2.Y - a1.Y) * (b2.X - b1.X);
        if (MathF.Abs(d) < 0.0001f)
        {
            t1 = t2 = 0;
            return false;
        }

        t1 = ((b1.Y - b2.Y) * (a1.X - b1.X) + (b2.X - b1.X) * (a1.Y - b1.Y)) / d;
        t2 = ((a1.Y - a2.Y) * (a1.X - b1.X) + (a2.X - a1.X) * (a1.Y - b1.Y)) / d;

        return t1 >= 0 && t1 < 1 && t2 >= 0 && t2 < 1;
    }


    static bool LinesIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
    {
        float d = (a2.X - a1.X) * (b2.Y - b1.Y) - (a2.Y - a1.Y) * (b2.X - b1.X);
        if (d == 0) return false;

        float u = ((b1.X - a1.X) * (b2.Y - b1.Y) - (b1.Y - a1.Y) * (b2.X - b1.X)) / d;
        float v = ((b1.X - a1.X) * (a2.Y - a1.Y) - (b1.Y - a1.Y) * (a2.X - a1.X)) / d;

        return u >= 0 && u <= 1 && v >= 0 && v <= 1;
    }

    static bool SweepAndPush(Vector2 from, Vector2 to, List<Vector2> targetPts, ref PolygonShape mover)
    {
        var dir = to - from;
        bool any = false;
        for (int i = 0; i < targetPts.Count; i++)
        {
            var b1 = targetPts[i];
            var b2 = targetPts[(i + 1) % targetPts.Count];
            if (LinesIntersect(from, to, b1, b2, out float t1, out _))
            {
                var push = (1f - t1) * dir;
                mover.Position -= push;
                any = true;
            }
        }
        return any;
    }

    private static bool SweepTestPolygon(Vector2 from, Vector2 to, PolygonShape poly, out Vector2 collisionPoint, out Vector2 collisionNormal)
    {
        collisionPoint = Vector2.Zero;
        collisionNormal = Vector2.Zero;

        var points = poly.GetTransformedPoints();
        float closestT = float.MaxValue;
        bool hit = false;

        for (int i = 0; i < points.Count; i++)
        {
            Vector2 p1 = points[i];
            Vector2 p2 = points[(i + 1) % points.Count];

            if (LinesIntersect(from, to, p1, p2, out float t1, out float t2))
            {
                if (t1 < closestT)
                {
                    closestT = t1;
                    // Calculer le point d'impact
                    collisionPoint = Vector2.Lerp(from, to, t1);

                    // Calculer la normale de l'arête (perpendiculaire)
                    Vector2 edge = p2 - p1;
                    collisionNormal = Vector2.Normalize(new Vector2(-edge.Y, edge.X));

                    hit = true;
                }
            }
        }

        return hit;
    }

    static bool ResolveCircleCircle(CircleShape a, CircleShape b, out Vector2 pushOut)
    {
        Vector2 delta = b.GetGlobalPosition() - a.GetGlobalPosition();
        float dist = delta.Length();
        float sumR = a.Radius + b.Radius;
        if (dist >= sumR) { pushOut = Vector2.Zero; return false; }

        float pen = sumR - dist;
        Vector2 normal = dist > 0 ? delta / dist : new Vector2(1, 0);
        pushOut = normal * pen;

        ApplyPush(a, b, pushOut);

        return true;
    }

    static bool ResolveSATCirclePolygon(CircleShape c, PolygonShape p, out Vector2 pushOut)
    {
        var pts = p.GetTransformedPoints();
        float minOverlap = float.MaxValue;
        Vector2 smallestAxis = Vector2.Zero;

        // 1) Axes des arêtes du polygone
        foreach (var edgePts in Enumerable.Range(0, pts.Count))
        {
            var a = pts[edgePts];
            var b = pts[(edgePts + 1) % pts.Count];
            var edge = b - a;
            var axis = Vector2.Normalize(new Vector2(-edge.Y, edge.X));

            var (minP, maxP) = ProjectPolygon(axis, pts);
            float projC = Vector2.Dot(axis, c.GetGlobalPosition());
            float minC = projC - c.Radius, maxC = projC + c.Radius;

            float overlap = MathF.Min(maxP, maxC) - MathF.Max(minP, minC);
            if (overlap <= 0) { pushOut = Vector2.Zero; return false; }
            if (overlap < minOverlap)
            {
                minOverlap = overlap;
                smallestAxis = axis;
            }
        }

        // 2) Axe entre centre du cercle et sommet le plus proche du polygone
        float bestDist = float.MaxValue;
        Vector2 closest = pts[0];
        foreach (var pt in pts)
        {
            float d2 = Vector2.DistanceSquared(c.GetGlobalPosition(), pt);
            if (d2 < bestDist) { bestDist = d2; closest = pt; }
        }
        var axis2 = Vector2.Normalize(closest - c.GetGlobalPosition());
        var (minP2, maxP2) = ProjectPolygon(axis2, pts);
        float projC2 = Vector2.Dot(axis2, c.GetGlobalPosition());
        float minC2 = projC2 - c.Radius, maxC2 = projC2 + c.Radius;
        float overlap2 = MathF.Min(maxP2, maxC2) - MathF.Max(minP2, minC2);
        if (overlap2 <= 0) { pushOut = Vector2.Zero; return false; }
        if (overlap2 < minOverlap)
        {
            minOverlap = overlap2;
            smallestAxis = axis2;
        }

        // Résolution du chevauchement
        Vector2 dir = p.GetGlobalPosition() - c.GetGlobalPosition();
        if (Vector2.Dot(dir, smallestAxis) < 0)
            smallestAxis *= -1;

        Vector2 resolution = smallestAxis * minOverlap;

        ApplyPush(c, p, resolution);

        pushOut = resolution;
        return true;
    }

    static void ApplyPush(Shape a, Shape b, Vector2 resolution)
    {
        Node GetRoot(Node node)
        {
            return (node.Parent != null) ? node.Parent : node;
        }

        var aRoot = GetRoot(a);
        var bRoot = GetRoot(b);

        if (!a.IsStatic && !b.IsStatic)
        {
            aRoot.Position -= resolution * 0.5f;
            bRoot.Position += resolution * 0.5f;
        }
        else if (!a.IsStatic)
        {
            aRoot.Position -= resolution;
        }
        else if (!b.IsStatic)
        {
            bRoot.Position += resolution;
        }
    }
}
