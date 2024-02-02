using Sandbox;
using Sandcube.Mth;
using System;

namespace Sandcube.Texturing;

public static class TextureExtensions
{
    public static Texture GetPart(this Texture texture, Rect rect)
    {
        var result = Texture.Create((int)rect.Width, (int)rect.Height).Finish();
        Color32[] data = new Color32[(int)rect.Width * (int)rect.Height];
        texture.GetPixels<Color32>(((int)rect.Left, (int)rect.Top, (int)rect.Width, (int)rect.Height), 0, 0, data, ImageFormat.RGBA8888);
        result.Update(data, 0, 0, (int)rect.Width, (int)rect.Height);
        return result;
    }

    public static void Update(this Texture @this, ReadOnlySpan<Color32> data, Rect rect) =>
        @this.Update(data, (int)rect.Left, (int)rect.Top, (int)rect.Width, (int)rect.Height);

    public static void Update(this Texture @this, Texture texture, Vector2Int position) =>
        @this.Update(texture.GetPixels(), position.x, position.y, texture.Width, texture.Height);
}
