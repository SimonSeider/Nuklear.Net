using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nuklear.Net.Examples.Common;
using Nuklear.Net.Rendering;
using NkKey = Nuklear.Net.Key;
using NkMouseButton = Nuklear.Net.MouseButton;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace Nuklear.Net.Examples.MonoGame;

public class Game1 : Game
{
    readonly GraphicsDeviceManager _graphics;
    Context? _context;
    MonoGameNuklearDevice? _device;
    float _sliderValue = 0.5f;
    int _clickCount;
    int _previousScrollWheelValue;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 960,
            PreferredBackBufferHeight = 540,
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.Title = "Nuklear.Net - MonoGame";
        IsFixedTimeStep = false;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _context = new Context();
        _device = new MonoGameNuklearDevice(GraphicsDevice);
        var metrics = DisplayMetrics.From(
            Window.ClientBounds.Width,
            Window.ClientBounds.Height,
            GraphicsDevice.PresentationParameters.BackBufferWidth,
            GraphicsDevice.PresentationParameters.BackBufferHeight);
        _context.Initialize(_device, NuklearRenderingOptions.CreatePixelPerfect(), metrics);
        ApplyDisplayMetrics();
    }

    void ApplyDisplayMetrics()
    {
        if (_context is null)
            return;

        _context.SetDisplayMetrics(DisplayMetrics.From(
            Window.ClientBounds.Width,
            Window.ClientBounds.Height,
            GraphicsDevice.PresentationParameters.BackBufferWidth,
            GraphicsDevice.PresentationParameters.BackBufferHeight));
    }

    protected override void Update(GameTime gameTime)
    {
        if (_context is null)
            return;

        _context.DeltaTimeSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        if (_context is null || _device is null)
            return;

        GraphicsDevice.Clear(new XnaColor(31, 36, 46));
        ApplyDisplayMetrics();
        _context.RunFrame(_device, MapInput, () => DemoUi.Build(_context, ref _sliderValue, ref _clickCount));

        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _device?.Dispose();
            _context?.Dispose();
        }

        base.Dispose(disposing);
    }

    void MapInput(InputFrame frame)
    {
        var mouse = Mouse.GetState();
        frame.MousePosition = new Vec2(mouse.X, mouse.Y);
        frame.SetMouseButton(NkMouseButton.Left, mouse.LeftButton == ButtonState.Pressed);
        frame.SetMouseButton(NkMouseButton.Right, mouse.RightButton == ButtonState.Pressed);
        frame.SetMouseButton(NkMouseButton.Middle, mouse.MiddleButton == ButtonState.Pressed);
        var scrollDelta = mouse.ScrollWheelValue - _previousScrollWheelValue;
        _previousScrollWheelValue = mouse.ScrollWheelValue;
        frame.ScrollDelta = new Vec2(0, scrollDelta / 120f);

        var keyboard = Keyboard.GetState();
        frame.SetKey(NkKey.Shift, keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift));
        frame.SetKey(NkKey.Ctrl, keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl));
        frame.SetKey(NkKey.Alt, keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt));
        frame.SetKey(NkKey.Enter, keyboard.IsKeyDown(Keys.Enter));
        frame.SetKey(NkKey.Backspace, keyboard.IsKeyDown(Keys.Back));
        frame.SetKey(NkKey.Tab, keyboard.IsKeyDown(Keys.Tab));
        frame.SetKey(NkKey.Up, keyboard.IsKeyDown(Keys.Up));
        frame.SetKey(NkKey.Down, keyboard.IsKeyDown(Keys.Down));
        frame.SetKey(NkKey.Left, keyboard.IsKeyDown(Keys.Left));
        frame.SetKey(NkKey.Right, keyboard.IsKeyDown(Keys.Right));
    }
}
