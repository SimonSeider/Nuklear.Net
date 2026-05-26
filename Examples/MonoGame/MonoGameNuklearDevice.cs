using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nuklear.Net.Rendering;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace Nuklear.Net.Examples.MonoGame;

public sealed class MonoGameNuklearDevice(GraphicsDevice device) : NuklearDevice<Texture2D>, IDisposable
{
    readonly RasterizerState _scissorRasterizer = new() { ScissorTestEnable = true };
    readonly BlendState _alphaBlend = BlendState.NonPremultiplied;
    BasicEffect? _effect;
    DynamicVertexBuffer? _vertexBuffer;
    IndexBuffer? _indexBuffer;
    NkDrawVertex[] _vertices = [];
    ushort[] _indices = [];
    int _logicalWidth = 1;
    int _logicalHeight = 1;
    int _framebufferWidth = 1;
    int _framebufferHeight = 1;
    float _framebufferScaleX = 1f;
    float _framebufferScaleY = 1f;
    bool _pixelPerfect;

    public override void ConfigureRendering(NuklearRenderingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _pixelPerfect = options.PixelPerfect;
    }

    public void Resize(DisplayMetrics metrics, bool usePhysicalPixels = false)
    {
        if (usePhysicalPixels)
        {
            _logicalWidth = Math.Max(1, metrics.FramebufferWidth);
            _logicalHeight = Math.Max(1, metrics.FramebufferHeight);
            _framebufferWidth = _logicalWidth;
            _framebufferHeight = _logicalHeight;
            _framebufferScaleX = 1f;
            _framebufferScaleY = 1f;
            UpdateProjection();
            return;
        }

        Resize(
            metrics.WindowWidth,
            metrics.WindowHeight,
            metrics.FramebufferWidth,
            metrics.FramebufferHeight);
    }

    public override void ConfigureDisplay(DisplayMetrics metrics, bool usePhysicalPixels) =>
        Resize(metrics, usePhysicalPixels);

    public void Resize(DisplayMetrics metrics) => Resize(metrics, usePhysicalPixels: false);

    public void Resize(int windowWidth, int windowHeight, int framebufferWidth, int framebufferHeight)
    {
        _logicalWidth = Math.Max(1, windowWidth);
        _logicalHeight = Math.Max(1, windowHeight);
        _framebufferWidth = Math.Max(1, framebufferWidth);
        _framebufferHeight = Math.Max(1, framebufferHeight);
        _framebufferScaleX = _framebufferWidth / (float)_logicalWidth;
        _framebufferScaleY = _framebufferHeight / (float)_logicalHeight;
        UpdateProjection();
    }

    public void Resize(int width, int height) => Resize(width, height, width, height);

    public override void Init()
    {
        base.Init();
        _effect = new BasicEffect(device)
        {
            VertexColorEnabled = true,
            TextureEnabled = true,
            View = Matrix.Identity,
            World = Matrix.Identity,
        };
        UpdateProjection();
    }

    void UpdateProjection()
    {
        if (_effect is null)
            return;

        _effect.Projection = Matrix.CreateOrthographicOffCenter(0, _logicalWidth, _logicalHeight, 0, 0, 1);
    }

    protected override Texture2D CreateTextureResource(int width, int height, ReadOnlySpan<byte> rgbaPixels)
    {
        var texture = new Texture2D(device, width, height);
        var colors = new XnaColor[width * height];
        for (var i = 0; i < colors.Length; i++)
        {
            var offset = i * 4;
            colors[i] = new XnaColor(rgbaPixels[offset], rgbaPixels[offset + 1], rgbaPixels[offset + 2], rgbaPixels[offset + 3]);
        }

        texture.SetData(colors);
        return texture;
    }

    public override void SetDrawBuffers(ReadOnlySpan<NkDrawVertex> vertices, ReadOnlySpan<ushort> indices)
    {
        _vertices = vertices.ToArray();
        _indices = indices.ToArray();
        if (_pixelPerfect)
            SnapVerticesToPixels(_vertices);

        if (_vertexBuffer is null || _vertexBuffer.VertexCount < _vertices.Length)
        {
            _vertexBuffer?.Dispose();
            _vertexBuffer = new DynamicVertexBuffer(device, NuklearVertexDeclaration.Instance, Math.Max(_vertices.Length, 256), BufferUsage.WriteOnly);
        }

        if (_indexBuffer is null || _indexBuffer.IndexCount < _indices.Length)
        {
            _indexBuffer?.Dispose();
            _indexBuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, Math.Max(_indices.Length, 512), BufferUsage.WriteOnly);
        }

        _vertexBuffer.SetData(ConvertVertices(_vertices), 0, _vertices.Length);
        _indexBuffer.SetData(_indices, 0, _indices.Length);
    }

    public override void BeginRender()
    {
        device.Viewport = new Viewport(0, 0, _framebufferWidth, _framebufferHeight);
        device.BlendState = _alphaBlend;
        device.DepthStencilState = DepthStencilState.None;
        device.RasterizerState = RasterizerState.CullNone;
    }

    public override void RenderDrawCommand(Texture2D texture, NuklearDrawCommand command)
    {
        if (_effect is null || _vertexBuffer is null || _indexBuffer is null)
            return;

        var w = (int)command.ClipRect.Width;
        var h = (int)command.ClipRect.Height;
        if (w <= 0 || h <= 0)
            return;

        var scissorX = Math.Max(0, (int)MathF.Floor(command.ClipRect.X * _framebufferScaleX));
        var scissorY = Math.Max(0, (int)MathF.Floor(command.ClipRect.Y * _framebufferScaleY));
        var scissorW = Math.Max(0, (int)MathF.Ceiling(command.ClipRect.Width * _framebufferScaleX));
        var scissorH = Math.Max(0, (int)MathF.Ceiling(command.ClipRect.Height * _framebufferScaleY));
        device.ScissorRectangle = new Rectangle(scissorX, scissorY, scissorW, scissorH);
        device.RasterizerState = _scissorRasterizer;

        device.SamplerStates[0] = _pixelPerfect ? SamplerState.PointClamp : SamplerState.LinearClamp;

        _effect.Texture = texture;
        _effect.TextureEnabled = true;
        device.SetVertexBuffer(_vertexBuffer);
        device.Indices = _indexBuffer;

        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            device.DrawIndexedPrimitives(
                PrimitiveType.TriangleList,
                0,
                (int)command.ElementOffset,
                (int)(command.ElementCount / 3));
        }
    }

    public override void EndRender()
    {
        device.ScissorRectangle = new Rectangle(0, 0, _framebufferWidth, _framebufferHeight);
        device.RasterizerState = RasterizerState.CullNone;
    }

    static void SnapVerticesToPixels(NkDrawVertex[] vertices)
    {
        for (var i = 0; i < vertices.Length; i++)
        {
            var vertex = vertices[i];
            vertex.PosX = MathF.Round(vertex.PosX);
            vertex.PosY = MathF.Round(vertex.PosY);
            vertices[i] = vertex;
        }
    }

    static NuklearVertex[] ConvertVertices(NkDrawVertex[] vertices)
    {
        var converted = new NuklearVertex[vertices.Length];
        for (var i = 0; i < vertices.Length; i++)
        {
            var vertex = vertices[i];
            converted[i] = new NuklearVertex(
                new Vector3(vertex.PosX, vertex.PosY, 0),
                new Vector2(vertex.U, vertex.V),
                new XnaColor(vertex.R, vertex.G, vertex.B, vertex.A));
        }

        return converted;
    }

    public void Dispose()
    {
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _effect?.Dispose();
        _scissorRasterizer.Dispose();

        foreach (var texture in EnumerateTextures())
            texture?.Dispose();
    }

    static class NuklearVertexDeclaration
    {
        public static readonly VertexDeclaration Instance = new(
        [
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(20, VertexElementFormat.Color, VertexElementUsage.Color, 0),
        ]);
    }

    struct NuklearVertex : IVertexType
    {
        public Vector3 Position;
        public Vector2 UV;
        public XnaColor Color;

        public NuklearVertex(Vector3 position, Vector2 uv, XnaColor color)
        {
            Position = position;
            UV = uv;
            Color = color;
        }

        public VertexDeclaration VertexDeclaration => NuklearVertexDeclaration.Instance;
    }
}
