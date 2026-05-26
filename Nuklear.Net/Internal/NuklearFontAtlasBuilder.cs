using System.Runtime.InteropServices;
using Nuklear.Net.Native;
using Nuklear.Net.Rendering;

namespace Nuklear.Net.Internal;

public sealed class NuklearFontAtlasBuilder : IDisposable
{
    readonly NkFontAtlas _atlas = new();
    readonly NkDrawNullTexture _nullTexture = new();
    bool _built;

    public NkFontAtlas Atlas => _atlas;

    internal NkDrawNullTexture NullTexture => _nullTexture;

    public void Build(
        NuklearDevice device,
        NkContext context,
        NuklearRenderingOptions options,
        DisplayMetrics metrics,
        bool usePhysicalPixels)
    {
        if (_built)
            return;

        ArgumentNullException.ThrowIfNull(options);

        device.ConfigureFontAtlas(_atlas);
        _atlas.InitDefault();
        _atlas.Begin();

        var (bakeSize, layoutSize) = GetFontSizes(options, metrics, usePhysicalPixels);
        NkFont? font;
        if (options.PixelPerfect)
        {
            using var config = nuklear.nk_font_config(bakeSize);
            config.PixelSnap = 1;
            config.OversampleH = 1;
            config.OversampleV = 1;
            font = _atlas.AddDefault(bakeSize, config);
        }
        else
        {
            font = _atlas.AddDefault(bakeSize, null);
        }

        var width = 0;
        var height = 0;
        var image = _atlas.Bake(ref width, ref height, NkFontAtlasFormat.NK_FONT_ATLAS_RGBA32);
        var pixels = CopyAtlasPixels(image, width, height);
        var textureHandle = device.CreateTextureHandle(width, height, pixels);
        _atlas.End(nuklear.NkHandleId(textureHandle), _nullTexture);

        var defaultFont = _atlas.DefaultFont ?? font;
        var fontHandle = defaultFont.Handle;
        fontHandle.Height = layoutSize;
        nuklear.NkStyleSetFont(context, fontHandle);
        _built = true;
    }

    internal static (float BakeSize, float LayoutSize) GetFontSizes(
        NuklearRenderingOptions options,
        DisplayMetrics metrics,
        bool usePhysicalPixels)
    {
        var baseSize = options.PixelPerfect ? MathF.Round(options.FontSize) : options.FontSize;
        if (!options.ScaleFontsWithDisplay || metrics.UniformScale <= 1.001f)
            return (baseSize, baseSize);

        if (usePhysicalPixels)
        {
            var layoutSize = baseSize * metrics.UniformScale;
            var bakeSize = options.SnapFractionalFontScale && metrics.IsFractionalScale
                ? baseSize * metrics.GetFontBakeScale(snapFractionalToInteger: true)
                : layoutSize;
            if (options.PixelPerfect)
            {
                layoutSize = MathF.Round(layoutSize);
                bakeSize = MathF.Round(bakeSize);
            }

            return (bakeSize, layoutSize);
        }

        var bakeScale = metrics.GetFontBakeScale(options.SnapFractionalFontScale);
        var logicalBakeSize = baseSize * bakeScale;
        if (options.PixelPerfect)
            logicalBakeSize = MathF.Round(logicalBakeSize);
        return (logicalBakeSize, baseSize);
    }

    static byte[] CopyAtlasPixels(IntPtr image, int width, int height)
    {
        var length = width * height * 4;
        var pixels = new byte[length];
        Marshal.Copy(image, pixels, 0, length);
        return pixels;
    }

    public void Dispose()
    {
        _atlas.Clear();
        _atlas.Dispose();
        _nullTexture.Dispose();
    }
}
