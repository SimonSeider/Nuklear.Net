using Nuklear.Net.Native;
using Nuklear.Net.Rendering;

namespace Nuklear.Net.Internal;

internal static class NativeMapping
{
    internal static Color ToColor(NkColor.__Internal color) =>
        Color.FromArgb(color.r, color.g, color.b, color.a);

    internal static Vec2 ToVec2(NkVec2i.__Internal value) =>
        new(value.x, value.y);

    internal static Rect ToRect(short x, short y, ushort width, ushort height) =>
        new(x, y, width, height);

    internal static Rect ToRect(NkRect rect) =>
        new(rect.X, rect.Y, rect.W, rect.H);

    internal static TextureHandle ToTextureHandle(NkHandle handle) =>
        TextureHandle.FromId(handle.Id);

    internal static FontHandle ToFontHandle(IntPtr fontPointer) =>
        fontPointer == IntPtr.Zero ? FontHandle.Empty : new FontHandle(fontPointer);
}
