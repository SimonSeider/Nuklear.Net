using Nuklear.Net.Native;

namespace Nuklear.Net;

public sealed class InputFrame
{
    readonly List<(MouseButton Button, bool Down)> _mouseButtons = [];
    readonly List<(Key Key, bool Down)> _keys = [];
    readonly List<char> _textInput = [];

    public float DisplayScale { get; set; } = 1f;

    public Vec2 MousePosition { get; set; }
    public Vec2 ScrollDelta { get; set; }

    public void SetMouseButton(MouseButton button, bool down) => _mouseButtons.Add((button, down));

    public void SetKey(Key key, bool down) => _keys.Add((key, down));

    public void AddText(char character) => _textInput.Add(character);

    public void AddText(ReadOnlySpan<char> characters)
    {
        foreach (var character in characters)
            _textInput.Add(character);
    }

    public void Reset()
    {
        MousePosition = Vec2.Zero;
        ScrollDelta = Vec2.Zero;
        _mouseButtons.Clear();
        _keys.Clear();
        _textInput.Clear();
    }

    internal void Apply(NkContext context)
    {
        var mouseX = (int)(MousePosition.X * DisplayScale);
        var mouseY = (int)(MousePosition.Y * DisplayScale);

        // We mirror full keystate each frame, matching the official GLFW backend.
        foreach (var (button, down) in _mouseButtons)
            nuklear.NkInputButton(context, (NkButtons)button, mouseX, mouseY, down);

        foreach (var (key, down) in _keys)
            nuklear.NkInputKey(context, (NkKeys)key, down);

        if (ScrollDelta.X != 0 || ScrollDelta.Y != 0)
        {
            var scroll = new NkVec2
            {
                X = ScrollDelta.X * DisplayScale,
                Y = ScrollDelta.Y * DisplayScale,
            };
            nuklear.NkInputScroll(context, scroll);
        }

        foreach (var character in _textInput)
        {
            if (character <= 127)
                nuklear.NkInputChar(context, (sbyte)character);
            else
                nuklear.NkInputUnicode(context, character);
        }

        // We apply motion last so nk_input_button cannot clear delta after it is computed.
        nuklear.NkInputMotion(context, mouseX, mouseY);
    }
}
