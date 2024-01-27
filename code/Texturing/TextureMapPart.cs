using Sandbox;

namespace Sandcube.Texturing;

public readonly record struct TextureMapPart(TextureMap TextureMap, Rect TextureRect) : IUvProvider
{
    private static readonly TextureMap _internalMap = new();
    public static readonly TextureMapPart Invalid = _internalMap.AddTexture(Texture.Invalid);
    public static readonly TextureMapPart White = _internalMap.AddTexture(Texture.White);
    public static readonly TextureMapPart Transparent = _internalMap.AddTexture(Texture.Transparent);

    public Rect Uv => new(TextureRect.TopLeft / TextureMap.Size, TextureRect.Size / TextureMap.Size);
    public Texture Texture => TextureMap.GetTexture(TextureRect);
}
