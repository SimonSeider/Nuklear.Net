namespace Nuklear.Net.Rendering;

public readonly struct TextureHandle(nint id, nint pointer = 0) : IEquatable<TextureHandle>
{
    public nint Id { get; } = id;
    public nint Pointer { get; } = pointer;

    public static TextureHandle FromPointer(nint pointer) => new(0, pointer);

    public static TextureHandle FromId(nint id) => new(id, 0);

    public bool IsEmpty => Id == 0 && Pointer == 0;

    public bool Equals(TextureHandle other) => Id == other.Id && Pointer == other.Pointer;

    public override bool Equals(object? obj) => obj is TextureHandle other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Id, Pointer);
}
