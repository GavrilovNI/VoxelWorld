using Sandbox;
using VoxelWorld.Mth;
using System;

namespace VoxelWorld.Texturing;

public static class TextureExtensions
{
    public static Texture GetPart(this Texture texture, RectInt rect)
    {
        var resultSize = rect.Size;
        var pixels = texture.GetPixels(rect);
        var result = Texture.Create(resultSize.x, resultSize.y, texture.ImageFormat).Finish();
        result.Update(pixels, 0, 0, resultSize.x, resultSize.y);
        return result;
    }

    public static Color32[] GetPixels(this Texture texture, RectInt rect, int slice = 0, int mip = 0)
    {
        var resultSize = rect.Size;
        Color32[] pixels = new Color32[resultSize.x * resultSize.y];
        texture.GetPixels<Color32>((rect.Left, rect.Top, resultSize.x, resultSize.y), slice, mip, pixels, texture.ImageFormat);
        return pixels;
    }

    public static void Update(this Texture @this, ReadOnlySpan<Color32> data, RectInt rect) =>
        @this.Update(data, rect.Left, rect.Top, rect.Width, rect.Height);

    public static void Update(this Texture @this, Texture texture, Vector2IntB position) =>
        @this.Update(texture.GetPixels(), position.x, position.y, texture.Width, texture.Height);
}
