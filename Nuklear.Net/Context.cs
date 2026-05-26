using System.Runtime.InteropServices;
using Nuklear.Net.Internal;
using Nuklear.Net.Native;
using Nuklear.Net.Rendering;

namespace Nuklear.Net;

public sealed class Context : IDisposable
{
    readonly NkContext _native = new();
    readonly NuklearConvertBuffers _convertBuffers = new();
    NuklearFontAtlasBuilder? _fontAtlas;
    NuklearDevice? _device;
    NuklearRenderingOptions _options = NuklearRenderingOptions.Default;
    DisplayMetrics _metrics = DisplayMetrics.Uniform(1, 1);
    bool _usePhysicalPixels;
    bool _disposed;

    public Context()
    {
        if (!nuklear.NkInitDefault(_native, null))
            throw new InvalidOperationException("Failed to initialize Nuklear context.");
    }

    public InputFrame Input { get; } = new();

    public NkContext Native => _native;

    public float UiScale { get; private set; } = 1f;

    public bool UsePhysicalPixels => _usePhysicalPixels;

    public DisplayMetrics DisplayMetrics => _metrics;

    public float DeltaTimeSeconds
    {
        get => _native.DeltaTimeSeconds;
        set => _native.DeltaTimeSeconds = value;
    }

    public void Initialize(NuklearDevice device, float fontSize = 14f) =>
        Initialize(device, new NuklearRenderingOptions { FontSize = fontSize });

    public void Initialize(NuklearDevice device, NuklearRenderingOptions options) =>
        Initialize(device, options, DisplayMetrics.Uniform(1, 1));

    public void Initialize(NuklearDevice device, NuklearRenderingOptions options, DisplayMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(options);
        if (_device is not null)
            throw new InvalidOperationException("Context is already initialized with a device.");

        _device = device;
        _options = options;
        ApplyDisplayMetrics(metrics, reconfigureDevice: false);

        device.ConfigureRendering(options);
        device.Init();
        _convertBuffers.Configure(options);
        device.ConfigureDisplay(_metrics, _usePhysicalPixels);
        _fontAtlas = new NuklearFontAtlasBuilder();
        _fontAtlas.Build(device, _native, options, _metrics, _usePhysicalPixels);
        _convertBuffers.SetNullTexture(_fontAtlas.NullTexture);
        ApplyUiScaling(force: true);
    }

    public void SetDisplayMetrics(DisplayMetrics metrics) =>
        ApplyDisplayMetrics(metrics, reconfigureDevice: true);

    public void BeginFrame()
    {
        Input.Reset();
        Input.DisplayScale = _usePhysicalPixels ? _metrics.UniformScale : 1f;
        nuklear.NkInputBegin(_native);
    }

    public void EndInput()
    {
        Input.Apply(_native);
        nuklear.NkInputEnd(_native);
    }

    public void RunFrame(NuklearDevice device, Action<InputFrame> prepareInput, Action buildUi)
    {
        BeginFrame();
        prepareInput(Input);
        EndInput();
        buildUi();
        try
        {
            Render(device);
        }
        finally
        {
            EndFrame();
        }
    }

    public void Render(NuklearDevice device)
    {
        EnsureDevice(device);
        DrawConverter.Render(_native, device, _convertBuffers);
    }

    public void EndFrame() => nuklear.NkClear(_native);

    public void Dispose()
    {
        if (_disposed)
            return;

        _fontAtlas?.Dispose();
        _convertBuffers.Dispose();
        nuklear.NkFree(_native);
        _disposed = true;
    }

    void ApplyDisplayMetrics(DisplayMetrics metrics, bool reconfigureDevice)
    {
        _metrics = metrics;
        _usePhysicalPixels = ShouldUsePhysicalPixels(_options, metrics);
        UiScale = ComputeUiScale(_options, metrics, _usePhysicalPixels);
        Input.DisplayScale = _usePhysicalPixels ? metrics.UniformScale : 1f;

        if (reconfigureDevice && _device is not null)
            _device.ConfigureDisplay(metrics, _usePhysicalPixels);

        if (_device is not null && _fontAtlas is not null)
            ApplyUiScaling(force: false);
    }

    void ApplyUiScaling(bool force)
    {
        if (!_options.ScaleUiWithDisplay)
            return;

        if (!_usePhysicalPixels)
            return;

        if (!force && MathF.Abs(UiScale - 1f) < 0.001f)
            return;

        NuklearStyleScaler.Apply(_native, UiScale);
    }

    static bool ShouldUsePhysicalPixels(NuklearRenderingOptions options, DisplayMetrics metrics) =>
        options.UsePhysicalPixelsOnFractionalScale
        && options.PixelPerfect
        && metrics.IsFractionalScale;

    static float ComputeUiScale(NuklearRenderingOptions options, DisplayMetrics metrics, bool usePhysicalPixels)
    {
        if (usePhysicalPixels)
            return metrics.UniformScale;

        if (options.ScaleUiWithDisplay && metrics.UniformScale > 1.001f)
            return metrics.UniformScale;

        return 1f;
    }

    void EnsureDevice(NuklearDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);
        if (_device is null)
            Initialize(device);
        else if (!ReferenceEquals(_device, device))
            throw new InvalidOperationException("Context was initialized with a different device.");
    }
}
