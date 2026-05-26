namespace Nuklear.Net;

public readonly struct Vec2(float x, float y) : IEquatable<Vec2>
{
    public float X { get; } = x;
    public float Y { get; } = y;

    public static Vec2 Zero => default;

    public float LengthSquared => X * X + Y * Y;

    public static bool operator ==(Vec2 left, Vec2 right) => left.Equals(right);

    public static bool operator !=(Vec2 left, Vec2 right) => !left.Equals(right);

    public static Vec2 operator +(Vec2 a, Vec2 b) => new(a.X + b.X, a.Y + b.Y);

    public static Vec2 operator -(Vec2 a, Vec2 b) => new(a.X - b.X, a.Y - b.Y);

    public static Vec2 operator *(Vec2 v, float scale) => new(v.X * scale, v.Y * scale);

    public bool Equals(Vec2 other) => X == other.X && Y == other.Y;

    public override bool Equals(object? obj) => obj is Vec2 other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public override string ToString() => $"({X}, {Y})";
}
