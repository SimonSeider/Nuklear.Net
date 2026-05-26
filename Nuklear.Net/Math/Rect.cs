namespace Nuklear.Net;

public readonly struct Rect(float x, float y, float width, float height) : IEquatable<Rect>
{
    public float X { get; } = x;
    public float Y { get; } = y;
    public float Width { get; } = width;
    public float Height { get; } = height;

    public float Right => X + Width;
    public float Bottom => Y + Height;

    public static Rect FromSize(float width, float height) => new(0, 0, width, height);

    public bool Contains(Vec2 point) =>
        point.X >= X && point.X < Right &&
        point.Y >= Y && point.Y < Bottom;

    public bool Equals(Rect other) =>
        X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;

    public override bool Equals(object? obj) => obj is Rect other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);

    public override string ToString() => $"({X}, {Y}, {Width}, {Height})";
}
