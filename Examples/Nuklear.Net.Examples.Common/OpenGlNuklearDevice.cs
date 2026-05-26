using Nuklear.Net.Rendering;

namespace Nuklear.Net.Examples.Common;

public abstract class OpenGlNuklearDevice : NuklearDevice
{
    readonly Dictionary<int, int> _textures = [];
    int _fallbackTexture;
    bool _initialized;
    protected bool PixelPerfect { get; private set; }

    protected abstract void InitializeGl();
    protected abstract void SetViewport(int framebufferWidth, int framebufferHeight);
    protected abstract int CreateGlTexture(int width, int height, ReadOnlySpan<byte> rgbaPixels);
    protected abstract void UploadBuffers(ReadOnlySpan<NkDrawVertex> vertices, ReadOnlySpan<ushort> indices);
    protected abstract void DrawIndexed(int texture, Rect clipRect, uint elementOffset, uint elementCount);

    protected int LogicalWidth { get; private set; } = 1;
    protected int LogicalHeight { get; private set; } = 1;
    protected int FramebufferWidth { get; private set; } = 1;
    protected int FramebufferHeight { get; private set; } = 1;
    protected float FramebufferScaleX { get; private set; } = 1f;
    protected float FramebufferScaleY { get; private set; } = 1f;

    public override void ConfigureRendering(NuklearRenderingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        PixelPerfect = options.PixelPerfect;
    }

    public override void ConfigureDisplay(DisplayMetrics metrics, bool usePhysicalPixels) =>
        Resize(metrics, usePhysicalPixels);

    public void Resize(DisplayMetrics metrics, bool usePhysicalPixels = false)
    {
        if (usePhysicalPixels)
        {
            LogicalWidth = Math.Max(1, metrics.FramebufferWidth);
            LogicalHeight = Math.Max(1, metrics.FramebufferHeight);
            FramebufferWidth = LogicalWidth;
            FramebufferHeight = LogicalHeight;
            FramebufferScaleX = 1f;
            FramebufferScaleY = 1f;
            return;
        }

        Resize(
            metrics.WindowWidth,
            metrics.WindowHeight,
            metrics.FramebufferWidth,
            metrics.FramebufferHeight);
    }

    public void Resize(DisplayMetrics metrics) => Resize(metrics, usePhysicalPixels: false);

    public void Resize(int windowWidth, int windowHeight, int framebufferWidth, int framebufferHeight)
    {
        LogicalWidth = Math.Max(1, windowWidth);
        LogicalHeight = Math.Max(1, windowHeight);
        FramebufferWidth = Math.Max(1, framebufferWidth);
        FramebufferHeight = Math.Max(1, framebufferHeight);
        FramebufferScaleX = FramebufferWidth / (float)LogicalWidth;
        FramebufferScaleY = FramebufferHeight / (float)LogicalHeight;
    }

    public void Resize(int width, int height) => Resize(width, height, width, height);

    public override void Init()
    {
        if (_initialized)
            return;

        InitializeGl();
        _fallbackTexture = CreateGlTexture(1, 1, [255, 255, 255, 255]);
        _initialized = true;
    }

    protected override void CreateTexture(int handle, int width, int height, ReadOnlySpan<byte> rgbaPixels) =>
        _textures[handle] = CreateGlTexture(width, height, rgbaPixels);

    public override void SetDrawBuffers(ReadOnlySpan<NkDrawVertex> vertices, ReadOnlySpan<ushort> indices) =>
        UploadBuffers(vertices, indices);

    public override void BeginRender() => SetViewport(FramebufferWidth, FramebufferHeight);

    public override void RenderDrawCommand(NuklearDrawCommand command)
    {
        var texture = _textures.TryGetValue((int)command.Texture.Id, out var handle) ? handle : _fallbackTexture;
        DrawIndexed(texture, command.ClipRect, command.ElementOffset, command.ElementCount);
    }
}
