using System.Numerics;
using Nuklear.Net.Examples.Common;
using Nuklear.Net.Rendering;
using Silk.NET.OpenGL;

namespace Nuklear.Net.Examples.SilkNet;

public unsafe sealed class SilkOpenGlDevice : OpenGlNuklearDevice, IDisposable
{
    readonly GL _gl;
    uint _vao;
    uint _vbo;
    uint _ebo;
    uint _program;
    int _projectionLocation;
    int _textureLocation;
    int _pixelPerfectLocation;
    bool _glInitialized;

    public SilkOpenGlDevice(GL gl) => _gl = gl;

    protected override void InitializeGl()
    {
        if (_glInitialized)
            return;

        _program = CreateProgram(VertexShaderSource, FragmentShaderSource);
        _projectionLocation = _gl.GetUniformLocation(_program, "uProjection");
        _textureLocation = _gl.GetUniformLocation(_program, "uTexture");
        _pixelPerfectLocation = _gl.GetUniformLocation(_program, "uPixelPerfect");
        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer();
        _ebo = _gl.GenBuffer();
        _glInitialized = true;
    }

    protected override void SetViewport(int framebufferWidth, int framebufferHeight) =>
        _gl.Viewport(0, 0, (uint)framebufferWidth, (uint)framebufferHeight);

    protected override int CreateGlTexture(int width, int height, ReadOnlySpan<byte> rgbaPixels)
    {
        var texture = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, texture);
        var minFilter = PixelPerfect ? GLEnum.Nearest : GLEnum.Linear;
        var magFilter = PixelPerfect ? GLEnum.Nearest : GLEnum.Linear;
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
        _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, rgbaPixels);
        _gl.BindTexture(TextureTarget.Texture2D, 0);
        return (int)texture;
    }

    protected override void UploadBuffers(ReadOnlySpan<NkDrawVertex> vertices, ReadOnlySpan<ushort> indices)
    {
        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        fixed (NkDrawVertex* vertexPtr = vertices)
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(NkDrawVertex)), vertexPtr, BufferUsageARB.StreamDraw);

        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        fixed (ushort* indexPtr = indices)
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(ushort)), indexPtr, BufferUsageARB.StreamDraw);

        var stride = (uint)sizeof(NkDrawVertex);
        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(0, 2, GLEnum.Float, false, stride, (void*)0);
        _gl.EnableVertexAttribArray(1);
        _gl.VertexAttribPointer(1, 2, GLEnum.Float, false, stride, (void*)(sizeof(float) * 2));
        _gl.EnableVertexAttribArray(2);
        _gl.VertexAttribPointer(2, 4, GLEnum.UnsignedByte, true, stride, (void*)(sizeof(float) * 4));
    }

    protected override void DrawIndexed(int texture, Rect clipRect, uint elementOffset, uint elementCount)
    {
        var w = (int)clipRect.Width;
        var h = (int)clipRect.Height;
        if (w <= 0 || h <= 0 || elementCount == 0)
            return;

        _gl.UseProgram(_program);
        _gl.UniformMatrix4(_projectionLocation, 1, false, CreateOrthographicMatrix(LogicalWidth, LogicalHeight));
        _gl.Uniform1(_textureLocation, 0);
        _gl.Uniform1(_pixelPerfectLocation, PixelPerfect ? 1 : 0);
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, (uint)texture);
        _gl.Enable(EnableCap.Blend);
        _gl.BlendEquation(BlendEquationModeEXT.FuncAdd);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        _gl.Disable(EnableCap.DepthTest);
        _gl.Disable(EnableCap.CullFace);
        ApplyScissor(clipRect, LogicalHeight, FramebufferScaleX, FramebufferScaleY);
        _gl.DrawElements(PrimitiveType.Triangles, elementCount, DrawElementsType.UnsignedShort, (void*)(elementOffset * sizeof(ushort)));
    }

    public override void BeginRender()
    {
        base.BeginRender();
        _gl.Enable(EnableCap.ScissorTest);
    }

    public override void EndRender()
    {
        _gl.Disable(EnableCap.ScissorTest);
        _gl.UseProgram(0);
    }

    void ApplyScissor(Rect clipRect, int logicalHeight, float scaleX, float scaleY)
    {
        var w = Math.Max(0, (int)MathF.Ceiling(clipRect.Width * scaleX));
        var h = Math.Max(0, (int)MathF.Ceiling(clipRect.Height * scaleY));
        var x = Math.Max(0, (int)MathF.Floor(clipRect.X * scaleX));
        var y = Math.Max(0, (int)MathF.Floor((logicalHeight - (clipRect.Y + clipRect.Height)) * scaleY));
        _gl.Scissor(x, y, (uint)w, (uint)h);
    }

    public void Dispose()
    {
        if (!_glInitialized)
            return;

        _gl.DeleteBuffer(_ebo);
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteVertexArray(_vao);
        _gl.DeleteProgram(_program);
        _glInitialized = false;
    }

    static float[] CreateOrthographicMatrix(int width, int height) =>
        Matrix4x4.CreateOrthographicOffCenter(0, width, height, 0, -1, 1).ToColumnMajor();

    uint CreateProgram(string vertexSource, string fragmentSource)
    {
        var vertex = CompileShader(ShaderType.VertexShader, vertexSource);
        var fragment = CompileShader(ShaderType.FragmentShader, fragmentSource);
        var program = _gl.CreateProgram();
        _gl.AttachShader(program, vertex);
        _gl.AttachShader(program, fragment);
        _gl.LinkProgram(program);
        _gl.GetProgram(program, GLEnum.LinkStatus, out var status);
        if (status == 0)
            throw new InvalidOperationException($"Program link failed: {_gl.GetProgramInfoLog(program)}");

        _gl.DeleteShader(vertex);
        _gl.DeleteShader(fragment);
        return program;
    }

    uint CompileShader(ShaderType type, string source)
    {
        var shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);
        _gl.GetShader(shader, ShaderParameterName.CompileStatus, out var status);
        if (status == 0)
            throw new InvalidOperationException($"Shader compile failed: {_gl.GetShaderInfoLog(shader)}");
        return shader;
    }

    const string VertexShaderSource = """
        #version 330 core
        layout(location = 0) in vec2 aPos;
        layout(location = 1) in vec2 aUV;
        layout(location = 2) in vec4 aColor;
        uniform mat4 uProjection;
        uniform int uPixelPerfect;
        out vec2 vUV;
        out vec4 vColor;
        void main()
        {
            vUV = aUV;
            vColor = aColor;
            vec2 pos = aPos;
            if (uPixelPerfect != 0)
                pos = floor(pos + vec2(0.5));
            gl_Position = uProjection * vec4(pos, 0.0, 1.0);
        }
        """;

    const string FragmentShaderSource = """
        #version 330 core
        in vec2 vUV;
        in vec4 vColor;
        uniform sampler2D uTexture;
        out vec4 FragColor;
        void main()
        {
            FragColor = vColor * texture(uTexture, vUV);
        }
        """;
}

internal static class MatrixExtensions
{
    public static float[] ToColumnMajor(this Matrix4x4 matrix) =>
    [
        matrix.M11, matrix.M12, matrix.M13, matrix.M14,
        matrix.M21, matrix.M22, matrix.M23, matrix.M24,
        matrix.M31, matrix.M32, matrix.M33, matrix.M34,
        matrix.M41, matrix.M42, matrix.M43, matrix.M44,
    ];
}
