namespace Nuklear.Net.Rendering;

public readonly struct NuklearDrawCommand(
    TextureHandle texture,
    Rect clipRect,
    uint elementOffset,
    uint elementCount)
{
    public TextureHandle Texture { get; } = texture;
    public Rect ClipRect { get; } = clipRect;
    public uint ElementOffset { get; } = elementOffset;
    public uint ElementCount { get; } = elementCount;
}
