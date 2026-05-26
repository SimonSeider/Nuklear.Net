using System.Numerics;
using Nuklear.Net.Examples.Common;
using Nuklear.Net.Rendering;
using OpenTK.Graphics.OpenGL4;

namespace Nuklear.Net.Examples.OpenTK;

public sealed class OpenTkGlDevice : OpenGlNuklearDevice, IDisposable
{
    const int VertexByteSize = sizeof(float) * 4 + sizeof(byte) * 4;
    int _vao;
    int _vbo;
    int _ebo;
    int _program;
    int _projectionLocation;
    int _textureLocation;
    int _pixelPerfectLocation;
    bool _glInitialized;

    protected override void InitializeGl()
    {
        if (_glInitialized)
            return;

        _program = CreateProgram(VertexShaderSource, FragmentShaderSource);
        _projectionLocation = GL.GetUniformLocation(_program, "uProjection");
        _textureLocation = GL.GetUniformLocation(_program, "uTexture");
        _pixelPerfectLocation = GL.GetUniformLocation(_program, "uPixelPerfect");
        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        _ebo = GL.GenBuffer();
        _glInitialized = true;
    }

    protected override void SetViewport(int framebufferWidth, int framebufferHeight) =>
        GL.Viewport(0, 0, framebufferWidth, framebufferHeight);

    protected override int CreateGlTexture(int width, int height, ReadOnlySpan<byte> rgbaPixels)
    {
        var texture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, texture);
        var minFilter = PixelPerfect ? TextureMinFilter.Nearest : TextureMinFilter.Linear;
        var magFilter = PixelPerfect ? TextureMagFilter.Nearest : TextureMagFilter.Linear;
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, rgbaPixels.ToArray());
        GL.BindTexture(TextureTarget.Texture2D, 0);
        return texture;
    }

    protected override void UploadBuffers(ReadOnlySpan<NkDrawVertex> vertices, ReadOnlySpan<ushort> indices)
    {
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * VertexByteSize, vertices.ToArray(), BufferUsageHint.StreamDraw);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(ushort), indices.ToArray(), BufferUsageHint.StreamDraw);

        var stride = VertexByteSize;
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, sizeof(float) * 2);
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, sizeof(float) * 4);
    }

    protected override void DrawIndexed(int texture, Rect clipRect, uint elementOffset, uint elementCount)
    {
        var w = (int)clipRect.Width;
        var h = (int)clipRect.Height;
        if (w <= 0 || h <= 0 || elementCount == 0)
            return;

        GL.UseProgram(_program);
        var matrix = CreateOrthographicMatrix(LogicalWidth, LogicalHeight);
        GL.UniformMatrix4(_projectionLocation, 1, false, ref matrix[0]);
        GL.Uniform1(_textureLocation, 0);
        GL.Uniform1(_pixelPerfectLocation, PixelPerfect ? 1 : 0);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, texture);
        GL.Enable(EnableCap.Blend);
        GL.BlendEquation(BlendEquationMode.FuncAdd);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);
        ApplyScissor(clipRect, LogicalHeight, FramebufferScaleX, FramebufferScaleY);
        GL.DrawElements(PrimitiveType.Triangles, (int)elementCount, DrawElementsType.UnsignedShort, (int)(elementOffset * sizeof(ushort)));
    }

    public override void BeginRender()
    {
        base.BeginRender();
        GL.Enable(EnableCap.ScissorTest);
    }

    public override void EndRender()
    {
        GL.Disable(EnableCap.ScissorTest);
        GL.UseProgram(0);
    }

    static void ApplyScissor(Rect clipRect, int logicalHeight, float scaleX, float scaleY)
    {
        var w = Math.Max(0, (int)MathF.Ceiling(clipRect.Width * scaleX));
        var h = Math.Max(0, (int)MathF.Ceiling(clipRect.Height * scaleY));
        var x = Math.Max(0, (int)MathF.Floor(clipRect.X * scaleX));
        var y = Math.Max(0, (int)MathF.Floor((logicalHeight - (clipRect.Y + clipRect.Height)) * scaleY));
        GL.Scissor(x, y, w, h);
    }

    public void Dispose()
    {
        if (!_glInitialized)
            return;

        GL.DeleteBuffer(_ebo);
        GL.DeleteBuffer(_vbo);
        GL.DeleteVertexArray(_vao);
        GL.DeleteProgram(_program);
        _glInitialized = false;
    }

    static float[] CreateOrthographicMatrix(int width, int height) =>
        Matrix4x4.CreateOrthographicOffCenter(0, width, height, 0, -1, 1).ToColumnMajor();

    static int CreateProgram(string vertexSource, string fragmentSource)
    {
        var vertex = CompileShader(ShaderType.VertexShader, vertexSource);
        var fragment = CompileShader(ShaderType.FragmentShader, fragmentSource);
        var program = GL.CreateProgram();
        GL.AttachShader(program, vertex);
        GL.AttachShader(program, fragment);
        GL.LinkProgram(program);
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var status);
        if (status == 0)
            throw new InvalidOperationException($"Program link failed: {GL.GetProgramInfoLog(program)}");

        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);
        return program;
    }

    static int CompileShader(ShaderType type, string source)
    {
        var shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);
        GL.GetShader(shader, ShaderParameter.CompileStatus, out var status);
        if (status == 0)
            throw new InvalidOperationException($"Shader compile failed: {GL.GetShaderInfoLog(shader)}");
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

internal static class OpenTkMatrixExtensions
{
    public static float[] ToColumnMajor(this Matrix4x4 matrix) =>
    [
        matrix.M11, matrix.M12, matrix.M13, matrix.M14,
        matrix.M21, matrix.M22, matrix.M23, matrix.M24,
        matrix.M31, matrix.M32, matrix.M33, matrix.M34,
        matrix.M41, matrix.M42, matrix.M43, matrix.M44,
    ];
}
