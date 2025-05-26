using Raylib_cs;

public readonly struct RenderContext
{
    public Color Color { get; }
    public float LineThickness { get; }
    public bool Fill { get; }

    public RenderContext(Color color, float lineThickness = 1f, bool fill = false)
    {
        Color = color;
        LineThickness = lineThickness;
        Fill = fill;
    }

    public static RenderContext Default => new(Color.Red);
}