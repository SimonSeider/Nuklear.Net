namespace Nuklear.Net.Rendering;

/// <summary>
/// Maps logical window coordinates to the physical framebuffer size used by the GPU.
/// </summary>
public readonly struct DisplayMetrics
{
    const float IntegerScaleTolerance = 0.01f;

    public int WindowWidth { get; init; }
    public int WindowHeight { get; init; }
    public int FramebufferWidth { get; init; }
    public int FramebufferHeight { get; init; }

    public float ScaleX => FramebufferWidth / (float)Math.Max(1, WindowWidth);

    public float ScaleY => FramebufferHeight / (float)Math.Max(1, WindowHeight);

    public float UniformScale => (ScaleX + ScaleY) * 0.5f;

    public bool IsIntegerScale =>
        IsNearInteger(ScaleX) && IsNearInteger(ScaleY);

    public bool IsFractionalScale => UniformScale > 1f + IntegerScaleTolerance && !IsIntegerScale;

    public float GetFontBakeScale(bool snapFractionalToInteger)
    {
        if (UniformScale <= 1f + IntegerScaleTolerance)
            return 1f;

        return snapFractionalToInteger && IsFractionalScale
            ? MathF.Round(UniformScale)
            : UniformScale;
    }

    public static DisplayMetrics Uniform(int width, int height) =>
        new()
        {
            WindowWidth = width,
            WindowHeight = height,
            FramebufferWidth = width,
            FramebufferHeight = height,
        };

    public static DisplayMetrics From(int windowWidth, int windowHeight, int framebufferWidth, int framebufferHeight) =>
        new()
        {
            WindowWidth = Math.Max(1, windowWidth),
            WindowHeight = Math.Max(1, windowHeight),
            FramebufferWidth = Math.Max(1, framebufferWidth),
            FramebufferHeight = Math.Max(1, framebufferHeight),
        };

    static bool IsNearInteger(float value) =>
        MathF.Abs(value - MathF.Round(value)) <= IntegerScaleTolerance;
}
