using System.Numerics;
using Raylib_cs;

public class Node
{
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; private set; } = true;

    public Vector2 Velocity = Vector2.Zero;

    private Vector2 _position = Vector2.Zero;
    public Vector2 Position
    {
        get => _position;
        set
        {
            _position = value;
            InvalidateGlobalTransform();
        }
    }

    private float _rotation = 0f;
    public float Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            InvalidateGlobalTransform();
        }
    }

    private Vector2 _scale = Vector2.One;
    public Vector2 Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            InvalidateGlobalTransform();
        }
    }

    /// <summary>
    /// Parent node.
    /// </summary>
    public Node? Parent { get; set; } = null;

    /// <summary>
    /// Childrens of this node.
    /// </summary>
    private readonly List<Node> _children = new();
    public IEnumerable<Node> GetChildren() => _children.AsReadOnly();

    /// <summary>
    /// Adds a child node to this node.
    /// The child node will be positioned relative to this node.
    /// </summary>
    /// <param name="child"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void AddChild(Node child)
    {
        if (child == null) throw new ArgumentNullException(nameof(child));
        if (_children.Contains(child)) return;

        // Sauvegarde de la position globale de l'enfant AVANT de l'attacher
        var globalPosition = child.GetGlobalPosition();

        _children.Add(child);
        child.Parent = this;
        child.InvalidateGlobalTransform();

        // Mise à jour de sa position locale pour conserver sa position globale identique
        child.SetGlobalPosition(globalPosition);
    }

    /// <summary>
    /// Removes a child node from this node.
    /// The child node will be positioned relative to the world space.
    /// </summary>
    /// <param name="child"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void RemoveChild(Node child)
    {
        if (child == null) throw new ArgumentNullException(nameof(child));
        if (!_children.Contains(child)) return;

        _children.Remove(child);
        child.Parent = null;
        child.InvalidateGlobalTransform();
    }

    public virtual Vector2 Size => Vector2.Zero;
    public AnchorEnum Anchor { get; set; } = AnchorEnum.TopLeft;

    public virtual Vector2 Origin
    {
        get
        {
            var size = Size;
            return Anchor switch
            {
                AnchorEnum.TopLeft => new Vector2(0, 0),
                AnchorEnum.TopCenter => new Vector2(size.X / 2f, 0),
                AnchorEnum.TopRight => new Vector2(size.X, 0),
                AnchorEnum.MiddleLeft => new Vector2(0, size.Y / 2f),
                AnchorEnum.Center => new Vector2(size.X / 2f, size.Y / 2f),
                AnchorEnum.MiddleRight => new Vector2(size.X, size.Y / 2f),
                AnchorEnum.BottomLeft => new Vector2(0, size.Y),
                AnchorEnum.BottomCenter => new Vector2(size.X / 2f, size.Y),
                AnchorEnum.BottomRight => new Vector2(size.X, size.Y),
                _ => Vector2.Zero
            };
        }
    }

    protected virtual void InvalidateGlobalTransform()
    {
        _isGlobalDirty = true;
        foreach (var child in _children)
            child.InvalidateGlobalTransform();
    }

    private bool _isGlobalDirty = true;
    private Transform2D _cachedGlobalTransform;
    public Transform2D GetGlobalTransform()
    {
        if (_isGlobalDirty)
        {
            // Transformation locale
            var local = new Transform2D(Position, Rotation, Scale);

            // Combinaison avec le parent (si existant)
            _cachedGlobalTransform = Parent != null
                ? local.Combine(Parent.GetGlobalTransform())
                : local;

            _isGlobalDirty = false;
        }
        return _cachedGlobalTransform;
    }

    public Vector2 GetGlobalPosition() => GetGlobalTransform().Position;
    public void SetGlobalPosition(Vector2 globalPosition)
    {
        var parentPos = Parent?.GetGlobalPosition() ?? Vector2.Zero;
        Position = globalPosition - parentPos;
    }

    public virtual void OnSweepCollision(Vector2 collisionPoint, Vector2 collisionNormal, float deltaTime)
    {
        // Comportement par défaut = rien
    }

    public virtual void OnCollision(Node other, Vector2 collisionNormal)
    {

    }


    public void Update(float deltaTime)
    {
        if (!IsActive) return;

        OnUpdate(deltaTime); // <-- Le parent d'abord
        foreach (var child in _children)
            child.Update(deltaTime);
    }

    public void Draw()
    {
        if (!IsActive) return;

        OnDraw();

        Vector2 globalPos = GetGlobalPosition();
        Raylib.DrawCircleV(globalPos, 4, Color.Black);
        Raylib.DrawText(Name, (int)globalPos.X + 5, (int)globalPos.Y - 5, 10, Color.Black);

        foreach (var child in _children)
            child.Draw();
    }

    public virtual void OnUpdate(float deltaTime)
    {
    }

    public virtual void OnDraw()
    {
    }
}