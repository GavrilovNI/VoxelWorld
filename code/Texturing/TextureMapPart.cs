using Sandbox;

namespace Sandcube.Texturing;

public readonly record struct TextureMapPart(TextureMap TextureMap, Rect TextureRect) : IUvProvider
{
    public Rect Uv => new(TextureRect.TopLeft / TextureMap.Size, TextureRect.Size / TextureMap.Size);
    public Texture Texture => TextureMap.GetTexture(TextureRect);
}
