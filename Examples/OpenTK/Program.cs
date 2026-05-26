using Nuklear.Net.Examples.Common;
using Nuklear.Net.Rendering;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using NkKey = Nuklear.Net.Key;
using NkMouseButton = Nuklear.Net.MouseButton;
using GlfwMouseButton = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;

namespace Nuklear.Net.Examples.OpenTK;

public static class Program
{
    public static void Main(string[] args) => new OpenTkExampleWindow().Run();
}

internal sealed class OpenTkExampleWindow : GameWindow
{
    Context? _context;
    OpenTkGlDevice? _device;
    float _sliderValue = 0.5f;
    int _clickCount;

    public OpenTkExampleWindow()
        : base(
            GameWindowSettings.Default,
            new NativeWindowSettings
            {
                ClientSize = new Vector2i(960, 540),
                Title = "Nuklear.Net - OpenTK",
            })
    {
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        _context = new Context();
        _device = new OpenTkGlDevice();
        var metrics = DisplayMetrics.From(ClientSize.X, ClientSize.Y, FramebufferSize.X, FramebufferSize.Y);
        _context.Initialize(_device, NuklearRenderingOptions.CreatePixelPerfect(), metrics);
        ApplyDisplayMetrics();
    }

    void ApplyDisplayMetrics()
    {
        if (_context is null)
            return;

        _context.SetDisplayMetrics(DisplayMetrics.From(
            ClientSize.X,
            ClientSize.Y,
            FramebufferSize.X,
            FramebufferSize.Y));
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        if (_context is not null)
            _context.DeltaTimeSeconds = (float)args.Time;
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        if (_context is null || _device is null)
            return;

        GL.ClearColor(0.12f, 0.14f, 0.18f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        _context.RunFrame(_device, frame => MapInput(frame, MouseState, KeyboardState), () => DemoUi.Build(_context, ref _sliderValue, ref _clickCount));
        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        ApplyDisplayMetrics();
    }

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);
        ApplyDisplayMetrics();
    }

    protected override void OnUnload()
    {
        _device?.Dispose();
        _context?.Dispose();
        base.OnUnload();
    }

    static void MapInput(InputFrame frame, MouseState mouse, KeyboardState keyboard)
    {
        frame.MousePosition = new Vec2(mouse.X, mouse.Y);
        frame.SetMouseButton(NkMouseButton.Left, mouse.IsButtonDown(GlfwMouseButton.Left));
        frame.SetMouseButton(NkMouseButton.Right, mouse.IsButtonDown(GlfwMouseButton.Right));
        frame.SetMouseButton(NkMouseButton.Middle, mouse.IsButtonDown(GlfwMouseButton.Middle));
        frame.ScrollDelta = new Vec2(mouse.ScrollDelta.X, mouse.ScrollDelta.Y);

        frame.SetKey(NkKey.Shift, keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift));
        frame.SetKey(NkKey.Ctrl, keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl));
        frame.SetKey(NkKey.Alt, keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt));
        frame.SetKey(NkKey.Enter, keyboard.IsKeyDown(Keys.Enter));
        frame.SetKey(NkKey.Backspace, keyboard.IsKeyDown(Keys.Backspace));
        frame.SetKey(NkKey.Tab, keyboard.IsKeyDown(Keys.Tab));
        frame.SetKey(NkKey.Up, keyboard.IsKeyDown(Keys.Up));
        frame.SetKey(NkKey.Down, keyboard.IsKeyDown(Keys.Down));
        frame.SetKey(NkKey.Left, keyboard.IsKeyDown(Keys.Left));
        frame.SetKey(NkKey.Right, keyboard.IsKeyDown(Keys.Right));
    }
}
