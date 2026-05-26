namespace Nuklear.Net.Rendering;

public abstract class NuklearDevice<TTexture> : NuklearDevice
{
    readonly List<TTexture> _textures = [default!];

    protected IReadOnlyList<TTexture> Textures => _textures;

    protected int RegisterTexture(TTexture texture)
    {
        _textures.Add(texture);
        return _textures.Count - 1;
    }

    protected TTexture GetTexture(int handle)
    {
        if (handle < 0 || handle >= _textures.Count)
            throw new ArgumentOutOfRangeException(nameof(handle));
        return _textures[handle];
    }

    protected IEnumerable<TTexture> EnumerateTextures()
    {
        for (var i = 0; i < _textures.Count; i++)
            yield return _textures[i];
    }

    public override void Init()
    {
        base.Init();
        if (_textures[0] is null)
            _textures[0] = CreateTextureResource(1, 1, [255, 255, 255, 255]);
    }

    protected sealed override void CreateTexture(int handle, int width, int height, ReadOnlySpan<byte> rgbaPixels)
    {
        while (_textures.Count <= handle)
            _textures.Add(default!);

        _textures[handle] = CreateTextureResource(width, height, rgbaPixels);
    }

    protected abstract TTexture CreateTextureResource(int width, int height, ReadOnlySpan<byte> rgbaPixels);

    public abstract void RenderDrawCommand(TTexture texture, NuklearDrawCommand command);

    public sealed override void RenderDrawCommand(NuklearDrawCommand command)
    {
        var handle = command.Texture.Id;
        RenderDrawCommand(GetTexture((int)handle), command);
    }
}
