# Nuklear.Net Examples

Sample applications that integrate `Nuklear.Net` with common graphics frameworks using the `NuklearDevice` render backend model.

## Prerequisites

Build the native library for your platform before running any example:

```bash
./scripts/build-native.sh linux-x64
```

## Architecture

Each backend implements `NuklearDevice` (or `NuklearDevice<TTexture>`):

- `CreateTextureHandle` uploads font atlas and widget textures
- `SetDrawBuffers` receives converted Nuklear vertex/index data from `nk_convert`
- `RenderDrawCommand` draws each batch with scissor + texture

Use `context.RunFrame(device, prepareInput, buildUi)` for the full Nuklear frame lifecycle (input → UI → convert → draw → clear).

## Projects

| Project | Framework | Device |
|---------|-----------|--------|
| `Nuklear.Net.Examples.SilkNet` | Silk.NET + OpenGL | `SilkOpenGlDevice` |
| `Nuklear.Net.Examples.OpenTK` | OpenTK 4 | `OpenTkGlDevice` |
| `Nuklear.Net.Examples.MonoGame` | MonoGame DesktopGL | `MonoGameNuklearDevice` |

## Run

```bash
dotnet run --project Examples/SilkNet/Nuklear.Net.Examples.SilkNet.csproj
dotnet run --project Examples/OpenTK/Nuklear.Net.Examples.OpenTK.csproj
dotnet run --project Examples/MonoGame/Nuklear.Net.Examples.MonoGame.csproj
```
