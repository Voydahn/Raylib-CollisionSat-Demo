using System.Numerics;
using Raylib_cs;

/// <summary>
/// Un Node spécialisé pour dessiner un sprite 2D.
/// </summary>
public sealed class SpriteNode : Node
{
    private readonly Texture2D _texture;
    private readonly Rectangle? _sourceRect;

    public override Vector2 Size { get; }

    /// <summary>
    /// Initialise un SpriteNode avec un texture complète.
    /// </summary>
    /// <param name="texture">La texture à afficher.</param>
    public SpriteNode(Texture2D texture)
        : this(texture, null)
    {
    }

    /// <summary>
    /// Initialise un SpriteNode avec une texture et une source rectangle optionnelle.
    /// </summary>
    /// <param name="texture">La texture à afficher.</param>
    /// <param name="sourceRect">La partie de la texture à afficher. Si null, la texture entière est utilisée.</param>
    public SpriteNode(Texture2D texture, Rectangle? sourceRect)
    {
        _texture = texture;
        _sourceRect = sourceRect;
        Name = "SpriteNode";

        if (sourceRect.HasValue)
            Size = new Vector2(sourceRect.Value.Width, sourceRect.Value.Height);
        else
            Size = new Vector2(texture.Width, texture.Height);
    }

    public override void OnDraw()
    {
        base.OnDraw();

        var transform = GetGlobalTransform();
        Vector2 origin = Origin * transform.Scale;

        Raylib.DrawTexturePro(
            _texture,
            _sourceRect ?? new Rectangle(0, 0, _texture.Width, _texture.Height),
            new Rectangle(transform.Position.X, transform.Position.Y, Size.X * transform.Scale.X, Size.Y * transform.Scale.Y),
            origin,
            MathHelper.ToDegrees(transform.Rotation),
            Color.White
        );
    }
}
