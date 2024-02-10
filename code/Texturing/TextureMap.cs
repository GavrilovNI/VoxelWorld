using Sandbox;
using Sandcube.Mth;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Sandcube.Texturing;

public class TextureMap
{
    public Texture Texture { get; private set; }

    protected TextureMapNode Nodes;
    protected Vector2Int MultipleOfExpand;
    protected Color32? FillColor;

    public Vector2Int Size => (Vector2Int)Texture.Size;

    protected List<(TextureMapPart textureMapPart, int frame, AnimatedTexture animatedTexture)> AnimatedTextures = new();
    protected float AnimatedTime = 0;

    public TextureMap(Vector2Int initialSize, Vector2Int multipleOfExpand, Color32? fillColor = null)
    {
        FillColor = fillColor;
        Texture = CreateTexture(initialSize);
        Nodes = new TextureMapNode(new RectInt(0, initialSize));
        MultipleOfExpand = multipleOfExpand;
    }

    public TextureMap(Vector2Int initialSize, Color32? fillColor = null) : this(initialSize, new Vector2Int(256, 256), fillColor)
    {
    }

    public TextureMap(Color32? fillColor) : this(new Vector2Int(256, 256), fillColor)
    {
    }

    public TextureMap() : this(null)
    {
    }

    protected virtual Texture CreateTexture(Vector2Int size)
    {
        var texture = Texture.Create(size.x, size.y).Finish();
        if(FillColor.HasValue)
            texture.Update(FillColor.Value, new Rect(0, size));
        return texture;
    }

    public Rect GetUv(RectInt textureRect) => new(textureRect.TopLeft / Texture.Size, textureRect.Size / Texture.Size);
    public Texture GetTexture(RectInt textureRect) => Texture.GetPart(textureRect);

    public TextureMapPart AddTexture(Texture texture)
    {
        var textureSize = (Vector2Int)texture.Size;
        if(!Nodes.TryTakeSpace(textureSize, out RectInt rect))
        {
            var newSize = new Vector2Int(Math.Max(Texture.Width, texture.Width), Texture.Height + texture.Height);
            var expandDelta = newSize - Size;
            expandDelta = (1f * expandDelta / MultipleOfExpand).Ceiling() * MultipleOfExpand;

            Expand(expandDelta);
            if(!Nodes.TryTakeSpace(textureSize, out rect))
                throw new InvalidOperationException("couldn't expand texture");
        }

        Texture.Update(texture.GetPixels(), rect);
        return new(this, rect);
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

    protected void Expand(Vector2Int delta)
    {
        if(delta.x < 0 || delta.y < 0)
            throw new ArgumentOutOfRangeException(nameof(delta), delta, "expand size can't be negative");

        var newTexture = CreateTexture(((Vector2Int)Texture.Size) + delta);
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

        public bool TryTakeSpace(Vector2Int size, out RectInt rect)
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

        public void Expand(Vector2Int size)
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
