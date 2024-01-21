using Sandbox;

namespace Sandcube.Texturing;

public readonly record struct TextureMapPart
{
    public TextureMap TextureMap { get; }
    public Rect TextureRect { get; }

    public Rect Uv => TextureMap.GetUv(TextureRect);
    public Texture Texture
    {
        get
        {
            var result = Texture.Create((int)TextureRect.Width, (int)TextureRect.Height).Finish();
            Color32[] data = new Color32[(int)TextureRect.Width * (int)TextureRect.Height];
            TextureMap.Texture.GetPixels<Color32>(((int)TextureRect.Left, (int)TextureRect.Top, (int)TextureRect.Width, (int)TextureRect.Height), 0, 0, data, ImageFormat.RGBA8888);
            result.Update(data, 0, 0, (int)TextureRect.Width, (int)TextureRect.Height);
            return result;
        }
    }

    public TextureMapPart(TextureMap textureMap, Rect textureRect)
    {
        TextureMap = textureMap;
        TextureRect = textureRect;
    }
}
