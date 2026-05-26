namespace Nuklear.Net.Rendering;

public readonly struct FontHandle(nint pointer) : IEquatable<FontHandle>
{
    public nint Pointer { get; } = pointer;

    public static FontHandle Empty => default;

    public bool IsEmpty => Pointer == 0;

    public bool Equals(FontHandle other) => Pointer == other.Pointer;

    public override bool Equals(object? obj) => obj is FontHandle other && Equals(other);

    public override int GetHashCode() => Pointer.GetHashCode();
}
