namespace Nuklear.Net.Rendering;

public sealed class NuklearRenderingOptions
{
    public static NuklearRenderingOptions Default { get; } = new();

    public static NuklearRenderingOptions CreatePixelPerfect(float fontSize = 14f) =>
        new()
        {
            PixelPerfect = true,
            FontSize = fontSize,
            ScaleFontsWithDisplay = true,
            ScaleUiWithDisplay = true,
            UsePhysicalPixelsOnFractionalScale = true,
            SnapFractionalFontScale = true,
        };

    public float FontSize { get; init; } = 14f;

    /// <summary>
    /// Snaps geometry and fonts to the pixel grid and disables shape anti-aliasing.
    /// </summary>
    public bool PixelPerfect { get; init; }

    /// <summary>
    /// Bakes the font atlas at a higher resolution on HiDPI displays.
    /// </summary>
    public bool ScaleFontsWithDisplay { get; init; }

    /// <summary>
    /// Scales widget padding, spacing, and borders with the active display scale.
    /// </summary>
    public bool ScaleUiWithDisplay { get; init; }

    /// <summary>
    /// Uses physical framebuffer coordinates when the display scale is fractional (125%, 150%, etc.).
    /// </summary>
    public bool UsePhysicalPixelsOnFractionalScale { get; init; }

    /// <summary>
    /// Rounds fractional display scale to the nearest integer when baking fonts.
    /// </summary>
    public bool SnapFractionalFontScale { get; init; }
}
