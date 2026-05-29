# Nuklear.Net

.NET 10 bindings for [Nuklear](https://github.com/Immediate-Mode-UI/Nuklear), a single-header immediate-mode GUI library written in C.

> **Status: incomplete.** Not all Nuklear widgets and API surface are wrapped. The high-level C# layer covers the core widget set and rendering pipeline, but several Nuklear features remain either unbound or untested. See [Missing features](#missing-features).

---
## Requirements

- .NET 10 SDK
- CMake ≥ 3.16
- A C99 compiler (GCC / Clang / MSVC)

## Running examples

```bash
dotnet run --project Examples/SilkNet/Nuklear.Net.Examples.SilkNet.csproj
dotnet run --project Examples/OpenTK/Nuklear.Net.Examples.OpenTK.csproj
dotnet run --project Examples/MonoGame/Nuklear.Net.Examples.MonoGame.csproj
```

See [`Examples/README.md`](Examples/README.md) for backend implementation details.

## Basic usage

```csharp
// 1. Create the context once.
using var ctx = new Context();
ctx.Initialize(device, NuklearRenderingOptions.CreatePixelPerfect());

// 2. Each frame:
ctx.RunFrame(device,
    input =>
    {
        input.MousePosition = new Vec2(mouseX, mouseY);
        input.SetMouseButton(MouseButton.Left, leftPressed);
    },
    () =>
    {
        // Nuklear UI built here via ctx.Native (NkContext)
    });
```

## Architecture

`NuklearDevice` is the rendering contract backends must implement:

- `CreateTexture` – uploads RGBA pixel data (font atlas, etc.)
- `SetDrawBuffers` – receives converted vertex/index buffers from `nk_convert`
- `RenderDrawCommand` – issues one draw call per Nuklear draw command (scissor + texture)

`Context` owns the Nuklear context lifetime, the font atlas, input dispatch, display metrics, and HiDPI scaling.

## Missing features

The following Nuklear functionality is not yet surfaced through the managed API:

- Custom font loading (only the built-in default font is exposed)
- `nk_plot` / `nk_plot_function` (charts)
- `nk_color_picker` / `nk_color_*` widgets
- `nk_combo` / `nk_combobox` variants beyond the basic binding
- `nk_popup_*` managed wrappers
- `nk_tooltip` convenience API
- `nk_style_*` full style struct access from C# (only uniform scaling is applied)
- Multi-font support and glyph range selection
- `nk_list_view` (field `end` is intentionally ignored by the generator)

Raw access through `ctx.Native` (the generated `NkContext`) is always available as a workaround for any missing wrapper.


