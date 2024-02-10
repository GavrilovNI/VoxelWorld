using Sandbox;
using Sandcube.Mth;
using System;

namespace Sandcube.Texturing;

public static class TextureExtensions
{
    public static Texture GetPart(this Texture texture, RectInt rect)
    {
        var resultSize = rect.Size;
        var result = Texture.Create(resultSize.x, resultSize.y, texture.ImageFormat).Finish();
        Color32[] data = new Color32[resultSize.x * resultSize.y];
        texture.GetPixels<Color32>((rect.Left, rect.Top, resultSize.x, resultSize.y), 0, 0, data, texture.ImageFormat);
        result.Update(data, 0, 0, resultSize.x, resultSize.y);
        return result;
    }

    public static void Update(this Texture @this, ReadOnlySpan<Color32> data, RectInt rect) =>
        @this.Update(data, rect.Left, rect.Top, rect.Width, rect.Height);

    public static void Update(this Texture @this, Texture texture, Vector2Int position) =>
        @this.Update(texture.GetPixels(), position.x, position.y, texture.Width, texture.Height);
}
