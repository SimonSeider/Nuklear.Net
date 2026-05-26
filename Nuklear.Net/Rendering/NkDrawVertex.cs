using System.Runtime.InteropServices;

namespace Nuklear.Net.Rendering;

[StructLayout(LayoutKind.Sequential, Size = 20)]
public struct NkDrawVertex
{
    public float PosX;
    public float PosY;
    public float U;
    public float V;
    public byte R;
    public byte G;
    public byte B;
    public byte A;

    public Vec2 Position
    {
        readonly get => new(PosX, PosY);
        set
        {
            PosX = value.X;
            PosY = value.Y;
        }
    }

    public Vec2 UV
    {
        readonly get => new(U, V);
        set
        {
            U = value.X;
            V = value.Y;
        }
    }

    public Color Color
    {
        readonly get => Color.FromArgb(R, G, B, A);
        set
        {
            R = value.R;
            G = value.G;
            B = value.B;
            A = value.A;
        }
    }
}
