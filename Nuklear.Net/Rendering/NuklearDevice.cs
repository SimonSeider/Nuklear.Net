using Nuklear.Net.Native;

namespace Nuklear.Net.Rendering;

public abstract class NuklearDevice
{
    int _nextTextureId = 1;

    public virtual void Init()
    {
    }

    public virtual void ConfigureFontAtlas(NkFontAtlas atlas)
    {
    }

    public virtual void ConfigureRendering(NuklearRenderingOptions options)
    {
    }

    public virtual void ConfigureDisplay(DisplayMetrics metrics, bool usePhysicalPixels)
    {
    }

    public int CreateTextureHandle(int width, int height, ReadOnlySpan<byte> rgbaPixels)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));

        var expected = width * height * 4;
        if (rgbaPixels.Length < expected)
            throw new ArgumentException($"Expected at least {expected} bytes of RGBA pixel data.", nameof(rgbaPixels));

        var handle = _nextTextureId++;
        CreateTexture(handle, width, height, rgbaPixels);
        return handle;
    }

    protected abstract void CreateTexture(int handle, int width, int height, ReadOnlySpan<byte> rgbaPixels);

    public abstract void SetDrawBuffers(ReadOnlySpan<NkDrawVertex> vertices, ReadOnlySpan<ushort> indices);

    public virtual void BeginRender()
    {
    }

    public abstract void RenderDrawCommand(NuklearDrawCommand command);

    public virtual void EndRender()
    {
    }

    protected static TextureHandle ToTextureHandle(int handle) => TextureHandle.FromId(handle);
}
