using Sandbox;
using VoxelWorld.Mth;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace VoxelWorld.Texturing;

public class TextureMap
{
    public Texture Texture { get; private set; }

    protected TextureMapNode Nodes;
    protected Vector2IntB MultipleOfExpand;
    protected Color32? FillColor;
    protected int Mips;
    protected int TextureBorderSize;

    public Vector2IntB Size => (Vector2IntB)Texture.Size;

    protected List<(TextureMapPart textureMapPart, int frame, AnimatedTexture animatedTexture)> AnimatedTextures = new();
    protected float AnimatedTime = 0;

    public TextureMap(Vector2IntB initialSize, Vector2IntB multipleOfExpand, int mips = 1, Color32? fillColor = null, int textureBorderSize = 0)
    {
        FillColor = fillColor;
        Mips = mips;
        TextureBorderSize = textureBorderSize;
        Texture = CreateTexture(initialSize);
        Nodes = new TextureMapNode(new RectInt(0, initialSize));
        MultipleOfExpand = multipleOfExpand;
    }

    public TextureMap(Vector2IntB initialSize, int mips = 1, Color32? fillColor = null, int textureBorderSize = 0) : this(initialSize, new Vector2IntB(256, 256), mips, fillColor, textureBorderSize)
    {
    }

    public TextureMap(int mips = 1, Color32? fillColor = null, int textureBorderSize = 0) : this(new Vector2IntB(256, 256), mips, fillColor, textureBorderSize)
    {
    }

    protected virtual Texture CreateTexture(Vector2IntB size)
    {
        var texture = Texture.Create(size.x, size.y).WithMips(Mips).Finish();
        if(FillColor.HasValue)
            texture.Update(FillColor.Value, new Rect(0, size));
        return texture;
    }

    public Rect GetUv(RectInt textureRect) => new(textureRect.TopLeft / Texture.Size, textureRect.Size / Texture.Size);
    public Texture GetTexture(RectInt textureRect) => Texture.GetPart(textureRect);

    public TextureMapPart AddTexture(Texture texture)
    {
        var textureSize = (Vector2IntB)texture.Size;
        var textureSizeWithBorders = textureSize + Vector2IntB.One * TextureBorderSize * 2;
        if(!Nodes.TryTakeSpace(textureSizeWithBorders, out RectInt rect))
        {
            var newSize = new Vector2IntB(Math.Max(Texture.Width, textureSizeWithBorders.x), Texture.Height + textureSizeWithBorders.y);
            var expandDelta = newSize - Size;
            expandDelta = (1f * expandDelta / MultipleOfExpand).Ceiling() * MultipleOfExpand;

            Expand(expandDelta);
            if(!Nodes.TryTakeSpace(textureSizeWithBorders, out rect))
                throw new InvalidOperationException("couldn't expand texture");
        }

        var realTextureRect = rect.Shrink(TextureBorderSize);
        Texture.Update(texture.GetPixels(), realTextureRect);

        AddTextureBorders(realTextureRect, TextureBorderSize);

        return new(this, realTextureRect);
    }

    protected void AddTextureBorders(RectInt textureRect, int borderSize = 1)
    {
        if(borderSize <= 0)
            return;

        var topRect = new RectInt(textureRect.Left, textureRect.Top, textureRect.Width, 1);
        var leftRect = new RectInt(textureRect.Left, textureRect.Top, 1, textureRect.Height);
        var bottomRect = new RectInt(textureRect.Left, textureRect.Bottom - 1, textureRect.Width, 1);
        var rightRect = new RectInt(textureRect.Right - 1, textureRect.Top, 1, textureRect.Height);

        var topPixels = Texture.GetPixels(topRect);
        var leftPixels = Texture.GetPixels(leftRect);
        var bottomPixels = Texture.GetPixels(bottomRect);
        var rightPixels = Texture.GetPixels(rightRect);

        for(int i = 1; i <= borderSize; ++i)
        {
            Texture.Update(topPixels, topRect + new Vector2IntB(0, -i));
            Texture.Update(leftPixels, leftRect + new Vector2IntB(-i, 0));
            Texture.Update(bottomPixels, bottomRect + new Vector2IntB(0, i));
            Texture.Update(rightPixels, rightRect + new Vector2IntB(i, 0));
        }

        Texture.Update(Texture.GetPixel(textureRect.Left, textureRect.Top),
            new RectInt(textureRect.TopLeft - borderSize, borderSize));
        Texture.Update(Texture.GetPixel(textureRect.Right - 1, textureRect.Top),
            new RectInt(new Vector2IntB(textureRect.Right, textureRect.Top - borderSize), borderSize));
        Texture.Update(Texture.GetPixel(textureRect.Left, textureRect.Bottom - 1),
            new RectInt(new Vector2IntB(textureRect.Left - borderSize, textureRect.Bottom), borderSize));
        Texture.Update(Texture.GetPixel(textureRect.Right - 1, textureRect.Bottom  -1),
            new RectInt(textureRect.BottomRight, borderSize));
    }

    public TextureMapPart AddAnimatedTexture(AnimatedTexture animatedTexture)
    {
        var texture = animatedTexture.GetFrameTexture(0);
        var textureMapPart = AddTexture(texture);
        AnimatedTextures.Add((textureMapPart, 0, animatedTexture));
        return textureMapPart;
    }

    public bool UpdateAnimatedTextures() => UpdateAnimatedTextures(Time.Delta);
    public bool UpdateAnimatedTextures(float deltaTime)
    {
        bool updated = false;
        AnimatedTime += deltaTime;
        for(int i = 0; i < AnimatedTextures.Count; ++i)
        {
            (TextureMapPart textureMapPart, int frame, AnimatedTexture animatedTexture) = AnimatedTextures[i];

            var nextFrame = animatedTexture.GetFrameIndex(AnimatedTime);
            if(nextFrame != frame)
            {
                Texture.Update(animatedTexture.GetFrameTexture(nextFrame).GetPixels(), textureMapPart.TextureRect);
                AnimatedTextures[i] = (textureMapPart, nextFrame, animatedTexture);
                updated = true;
            }
        }
        return updated;
    }

    protected void Expand(Vector2IntB delta)
    {
        if(delta.x < 0 || delta.y < 0)
            throw new ArgumentOutOfRangeException(nameof(delta), delta, "expand size can't be negative");

        var newTexture = CreateTexture(((Vector2IntB)Texture.Size) + delta);
        newTexture.Update(Texture.GetPixels(), 0, 0, Texture.Width, Texture.Height);
        Texture = newTexture;

        Nodes.Expand(delta);
    }


    public class TextureMapNode
    {
        protected RectInt Rect;
        protected RectInt? TakenRect = null;
        protected TextureMapNode? RightChild = null;
        protected TextureMapNode? BottomChild = null;
        protected bool IsFull = false;

        public TextureMapNode(RectInt rect)
        {
            Rect = rect;
        }

        public bool TryTakeSpace(Vector2IntB size, out RectInt rect)
        {
            if(IsFull || size.x > Rect.Width || size.y > Rect.Height)
            {
                rect = default;
                return false;
            }

            if(TakenRect == null)
            {
                TakenRect = rect = new RectInt(Rect.TopLeft, size);

                if(Rect.Width > size.x)
                    RightChild = new(new RectInt(rect.Right, Rect.Top, Rect.Width - size.x, size.y));
                if(Rect.Height > size.y)
                    BottomChild = new(new RectInt(Rect.Left, Rect.Top + size.y, Rect.Width, Rect.Height - size.y));

                IsFull = RightChild == null && BottomChild == null;
                return true;
            }
            else
            {
                if(RightChild?.TryTakeSpace(size, out rect) ?? false)
                {
                    IsFull = RightChild.IsFull && (BottomChild == null || BottomChild.IsFull);
                    return true;
                }

                if(BottomChild?.TryTakeSpace(size, out rect) ?? false)
                {
                    IsFull = BottomChild.IsFull && (RightChild == null || RightChild.IsFull);
                    return true;
                }
            }

            rect = default;
            return false;
        }

        public void Expand(Vector2IntB size)
        {
            if(size.x <= 0 && size.y <= 0)
                return;

            IsFull = false;
            Rect = Rect.Grow(0, 0, size.x, size.y);

            if(TakenRect != null)
            {
                var takenRect = TakenRect.Value;

                if(size.x > 0)
                {
                    if(RightChild == null)
                        RightChild = new(new RectInt(takenRect.Right, Rect.Top, size.x, takenRect.Height));
                    else
                        RightChild.Expand(size.WithY(0));
                }

                if(BottomChild == null)
                    BottomChild = new(new RectInt(Rect.Left, takenRect.Bottom, Rect.Width, size.y));
                else
                    BottomChild.Expand(size);
            }
        }
    }
}
