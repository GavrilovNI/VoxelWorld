﻿using Sandbox;
using VoxelWorld.Mth;
using System;
using System.Linq;

namespace VoxelWorld.Texturing;

public class AnimatedTexture
{
    protected Texture Texture { get; }
    protected Vector2IntB AtlasSize { get; }
    protected float[] FramesLength { get; }

    protected Vector2IntB FrameTextureSize { get; }
    protected float FullTime { get; }

    public float FramesCount => FramesLength.Length;

    protected Texture?[] Frames { get; }

    public AnimatedTexture(Texture texture, Vector2IntB atlasSize, float[] framesLength)
    {
        if(framesLength.Length < 1)
            throw new ArgumentException("Frames count should not be 0 or negative", nameof(framesLength));
        if(framesLength.Length > atlasSize.x * atlasSize.y)
            throw new ArgumentException("Frames count should be less or equal to the number of textures in atlas", nameof(framesLength));

        Texture = texture;
        AtlasSize = atlasSize;
        FramesLength = framesLength;

        FrameTextureSize = (Texture.Size / AtlasSize).Floor();
        Frames = new Texture?[framesLength.Length];
        FullTime = framesLength.Sum();
    }

    protected RectInt GetFrameRect(int index)
    {
        Vector2IntB position = new(index % AtlasSize.x, index / AtlasSize.x);
        position *= FrameTextureSize;
        return new RectInt(position, FrameTextureSize);
    }

    public Texture GetFrameTexture(int index)
    {
        if(index < 0 || index >= FramesCount)
            throw new ArgumentOutOfRangeException(nameof(index));

        var texture = Frames[index];
        if(texture is not null)
            return texture;

        texture = Texture.GetPart(GetFrameRect(index));
        Frames[index] = texture;
        return texture;
    }

    public int GetFrameIndex(float time)
    {
        time %= FullTime;

        for(int i = 0; i < FramesLength.Length; ++i)
        {
            var currentLength = FramesLength[i];
            if(time < currentLength)
                return i;
            time -= currentLength;
        }

        return 0;
    }
}
