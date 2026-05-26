namespace Nuklear.Net;

public readonly struct Color(byte r, byte g, byte b, byte a = 255) : IEquatable<Color>
{
    public byte R { get; } = r;
    public byte G { get; } = g;
    public byte B { get; } = b;
    public byte A { get; } = a;

    public static Color White => new(255, 255, 255);
    public static Color Black => new(0, 0, 0);
    public static Color Transparent => new(0, 0, 0, 0);

    public static Color FromArgb(byte r, byte g, byte b, byte a = 255) => new(r, g, b, a);

    public static Color FromRgba32(uint rgba) =>
        new(
            (byte)((rgba >> 24) & 0xFF),
            (byte)((rgba >> 16) & 0xFF),
            (byte)((rgba >> 8) & 0xFF),
            (byte)(rgba & 0xFF));

    public uint ToRgba32() => ((uint)R << 24) | ((uint)G << 16) | ((uint)B << 8) | A;

    public bool Equals(Color other) => R == other.R && G == other.G && B == other.B && A == other.A;

    public override bool Equals(object? obj) => obj is Color other && Equals(other);

    public override int GetHashCode() => ToRgba32().GetHashCode();

    public override string ToString() => $"RGBA({R}, {G}, {B}, {A})";
}
