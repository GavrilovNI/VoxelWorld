using Sandbox;
using VoxelWorld.Mth;

namespace VoxelWorld.Texturing;

public readonly record struct TextureMapPart(TextureMap TextureMap, RectInt TextureRect) : IUvProvider
{
    public Rect Uv => new(1f * TextureRect.TopLeft / TextureMap.Size, 1f * TextureRect.Size / TextureMap.Size);
    public Texture Texture => TextureMap.GetTexture(TextureRect);
}
