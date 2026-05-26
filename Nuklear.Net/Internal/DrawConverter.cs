using System.Runtime.InteropServices;
using Nuklear.Net.Native;
using Nuklear.Net.Rendering;

namespace Nuklear.Net.Internal;

internal static unsafe class DrawConverter
{
    const int VertexSize = sizeof(float) * 4 + sizeof(byte) * 4;

    public static void Render(NkContext context, NuklearDevice device, NuklearConvertBuffers buffers)
    {
        buffers.Commands.Clear();
        buffers.Vertices.Clear();
        buffers.Elements.Clear();

        var result = (NkConvertResult)nuklear.NkConvert(context, buffers.Commands, buffers.Vertices, buffers.Elements, buffers.Config);
        if (result != NkConvertResult.NK_CONVERT_SUCCESS)
            throw new InvalidOperationException($"nk_convert failed: {result}");

        var vertexCount = (int)(buffers.Vertices.Needed / (ulong)VertexSize);
        var indexCount = (int)(buffers.Elements.Needed / sizeof(ushort));
        if (vertexCount == 0 || indexCount == 0)
            return;

        var vertices = new NkDrawVertex[vertexCount];
        var vertexPtr = (NkDrawVertex*)buffers.Vertices.MemoryConst;
        for (var i = 0; i < vertexCount; i++)
            vertices[i] = vertexPtr[i];

        var indices = new ushort[indexCount];
        var indexPtr = (ushort*)buffers.Elements.MemoryConst;
        for (var i = 0; i < indexCount; i++)
            indices[i] = indexPtr[i];

        device.SetDrawBuffers(vertices, indices);
        device.BeginRender();

        var offset = 0u;
        var command = nuklear.NkDrawBegin(context, buffers.Commands);
        while (command is not null && command.__Instance != IntPtr.Zero)
        {
            if (command.ElemCount > 0)
            {
                var clip = NativeMapping.ToRect(command.ClipRect);
                var texture = NativeMapping.ToTextureHandle(command.Texture);
                device.RenderDrawCommand(new NuklearDrawCommand(texture, clip, offset, command.ElemCount));
                offset += command.ElemCount;
            }

            command = nuklear.NkDrawNext(command, buffers.Commands, context);
        }

        device.EndRender();
    }
}

internal sealed unsafe class NuklearConvertBuffers : IDisposable
{
    const int VertexByteSize = sizeof(float) * 4 + sizeof(byte) * 4;
    const int LayoutElementByteSize = 16;
    readonly IntPtr _layoutMemory;
    public NkBuffer Commands { get; } = new();
    public NkBuffer Vertices { get; } = new();
    public NkBuffer Elements { get; } = new();
    public NkConvertConfig Config { get; } = new();

    public NuklearConvertBuffers()
    {
        _layoutMemory = Marshal.AllocHGlobal(LayoutElementByteSize * 4);
        var layout = (NkDrawVertexLayoutElement.__Internal*)_layoutMemory;
        layout[0] = new NkDrawVertexLayoutElement.__Internal
        {
            attribute = NkDrawVertexLayoutAttribute.NK_VERTEX_POSITION,
            format = NkDrawVertexLayoutFormat.NK_FORMAT_FLOAT,
            offset = 0,
        };
        layout[1] = new NkDrawVertexLayoutElement.__Internal
        {
            attribute = NkDrawVertexLayoutAttribute.NK_VERTEX_TEXCOORD,
            format = NkDrawVertexLayoutFormat.NK_FORMAT_FLOAT,
            offset = (ulong)(sizeof(float) * 2),
        };
        layout[2] = new NkDrawVertexLayoutElement.__Internal
        {
            attribute = NkDrawVertexLayoutAttribute.NK_VERTEX_COLOR,
            format = NkDrawVertexLayoutFormat.NK_FORMAT_R8G8B8A8,
            offset = (ulong)(sizeof(float) * 4),
        };
        layout[3] = new NkDrawVertexLayoutElement.__Internal
        {
            attribute = NkDrawVertexLayoutAttribute.NK_VERTEX_ATTRIBUTE_COUNT,
            format = NkDrawVertexLayoutFormat.NK_FORMAT_COUNT,
            offset = 0,
        };

        Commands.InitDefault();
        Vertices.InitDefault();
        Elements.InitDefault();

        Config.GlobalAlpha = 1f;
        Config.CircleSegmentCount = 22;
        Config.CurveSegmentCount = 22;
        Config.ArcSegmentCount = 22;
        Config.VertexSize = (ulong)VertexByteSize;
        Config.VertexAlignment = 4;
        ((NkConvertConfig.__Internal*)Config.__Instance)->vertex_layout = _layoutMemory;
        Configure(NuklearRenderingOptions.Default);
    }

    public void Configure(NuklearRenderingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var antiAlias = options.PixelPerfect
            ? NkAntiAliasing.NK_ANTI_ALIASING_OFF
            : NkAntiAliasing.NK_ANTI_ALIASING_ON;
        Config.LineAA = antiAlias;
        Config.ShapeAA = antiAlias;
    }

    public void SetNullTexture(NkDrawNullTexture nullTexture)
    {
        ArgumentNullException.ThrowIfNull(nullTexture);
        Config.TexNull = nullTexture;
    }

    public void Dispose()
    {
        Commands.Free();
        Commands.Dispose();
        Vertices.Free();
        Vertices.Dispose();
        Elements.Free();
        Elements.Dispose();
        Config.Dispose();
        Marshal.FreeHGlobal(_layoutMemory);
    }
}
