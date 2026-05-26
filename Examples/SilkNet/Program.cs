using Nuklear.Net.Examples.Common;
using Nuklear.Net.Rendering;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using NkKey = Nuklear.Net.Key;
using NkMouseButton = Nuklear.Net.MouseButton;
using SilkKey = Silk.NET.Input.Key;
using SilkMouseButton = Silk.NET.Input.MouseButton;

namespace Nuklear.Net.Examples.SilkNet;

public static class Program
{
    public static void Main(string[] args)
    {
        var options = WindowOptions.Default with
        {
            Size = new Vector2D<int>(960, 540),
            Title = "Nuklear.Net - Silk.NET",
            API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible, new APIVersion(3, 3)),
        };

        using var window = Window.Create(options);
        var app = new SilkExampleApp(window);
        window.Load += app.OnLoad;
        window.Update += app.OnUpdate;
        window.Render += app.OnRender;
        window.Resize += app.OnResize;
        window.FramebufferResize += app.OnFramebufferResize;
        window.Closing += app.OnClosing;
        window.Run();
        app.Dispose();
    }
}

internal sealed class SilkExampleApp : IDisposable
{
    readonly IWindow _window;
    GL? _gl;
    IInputContext? _input;
    Context? _context;
    SilkOpenGlDevice? _device;
    float _sliderValue = 0.5f;
    int _clickCount;

    public SilkExampleApp(IWindow window) => _window = window;

    public void OnLoad()
    {
        _gl = GL.GetApi(_window);
        _input = _window.CreateInput();
        _context = new Context();
        _device = new SilkOpenGlDevice(_gl);
        var metrics = DisplayMetrics.From(_window.Size.X, _window.Size.Y, _window.FramebufferSize.X, _window.FramebufferSize.Y);
        _context.Initialize(_device, NuklearRenderingOptions.CreatePixelPerfect(), metrics);
        ApplyDisplayMetrics();
    }

    void ApplyDisplayMetrics()
    {
        if (_context is null)
            return;

        _context.SetDisplayMetrics(DisplayMetrics.From(
            _window.Size.X,
            _window.Size.Y,
            _window.FramebufferSize.X,
            _window.FramebufferSize.Y));
    }

    public void OnUpdate(double deltaSeconds)
    {
        if (_context is not null)
            _context.DeltaTimeSeconds = (float)deltaSeconds;
    }

    public void OnRender(double deltaSeconds)
    {
        if (_gl is null || _context is null || _device is null || _input is null)
            return;

        _gl.ClearColor(0.12f, 0.14f, 0.18f, 1f);
        _gl.Clear(ClearBufferMask.ColorBufferBit);
        _context.RunFrame(_device, MapInput, () => DemoUi.Build(_context, ref _sliderValue, ref _clickCount));
    }

    public void OnResize(Vector2D<int> size) => ApplyDisplayMetrics();

    public void OnFramebufferResize(Vector2D<int> size) => ApplyDisplayMetrics();

    public void OnClosing() => Dispose();

    void MapInput(InputFrame frame)
    {
        if (_input is null)
            return;

        if (_input.Mice.Count > 0)
        {
            var mouse = _input.Mice[0];
            frame.MousePosition = new Vec2(mouse.Position.X, mouse.Position.Y);
            frame.SetMouseButton(NkMouseButton.Left, mouse.IsButtonPressed(SilkMouseButton.Left));
            frame.SetMouseButton(NkMouseButton.Right, mouse.IsButtonPressed(SilkMouseButton.Right));
            frame.SetMouseButton(NkMouseButton.Middle, mouse.IsButtonPressed(SilkMouseButton.Middle));

            if (mouse.ScrollWheels.Count > 0)
                frame.ScrollDelta = new Vec2(mouse.ScrollWheels[0].X, mouse.ScrollWheels[0].Y);
        }

        foreach (var keyboard in _input.Keyboards)
        {
            frame.SetKey(NkKey.Shift, keyboard.IsKeyPressed(SilkKey.ShiftLeft) || keyboard.IsKeyPressed(SilkKey.ShiftRight));
            frame.SetKey(NkKey.Ctrl, keyboard.IsKeyPressed(SilkKey.ControlLeft) || keyboard.IsKeyPressed(SilkKey.ControlRight));
            frame.SetKey(NkKey.Alt, keyboard.IsKeyPressed(SilkKey.AltLeft) || keyboard.IsKeyPressed(SilkKey.AltRight));
            frame.SetKey(NkKey.Enter, keyboard.IsKeyPressed(SilkKey.Enter));
            frame.SetKey(NkKey.Backspace, keyboard.IsKeyPressed(SilkKey.Backspace));
            frame.SetKey(NkKey.Tab, keyboard.IsKeyPressed(SilkKey.Tab));
            frame.SetKey(NkKey.Up, keyboard.IsKeyPressed(SilkKey.Up));
            frame.SetKey(NkKey.Down, keyboard.IsKeyPressed(SilkKey.Down));
            frame.SetKey(NkKey.Left, keyboard.IsKeyPressed(SilkKey.Left));
            frame.SetKey(NkKey.Right, keyboard.IsKeyPressed(SilkKey.Right));
        }
    }

    public void Dispose()
    {
        _device?.Dispose();
        _context?.Dispose();
        _input?.Dispose();
    }
}
