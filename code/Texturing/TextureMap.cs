using Sandbox;
using Sandcube.Mth;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sandcube.Texturing;

public class TextureMap
{
    public Texture Texture { get; private set; } = Texture.Create(1, 1).Finish();

    protected TextureMapNode Nodes;
    protected Vector2Int MultipleOfExpand;

    public Vector2Int Size => (Vector2Int)Texture.Size;

    protected List<(TextureMapPart textureMapPart, int frame, AnimatedTexture animatedTexture)> AnimatedTextures = new();
    protected float AnimatedTime = 0;

    public TextureMap(Vector2Int initialSize, Vector2Int multipleOfExpand)
    {
        Texture = Texture.Create(initialSize.x, initialSize.y).Finish();
        Nodes = new TextureMapNode(new Rect(0, 0, initialSize.x, initialSize.y));
        MultipleOfExpand = multipleOfExpand;
    }

    public TextureMap(Vector2Int initialSize) : this(initialSize, new Vector2Int(256, 256))
    {
    }

    public TextureMap() : this(new Vector2Int(256, 256))
    {
    }

    public Rect GetUv(Rect textureRect) => new(textureRect.TopLeft / Texture.Size, textureRect.Size / Texture.Size);
    public Texture GetTexture(Rect textureRect) => Texture.GetPart(textureRect);

    public TextureMapPart AddTexture(Texture texture)
    {
        var textureSize = (Vector2Int)texture.Size;
        if(!Nodes.TryTakeSpace(textureSize, out Rect rect))
        {
            var newSize = new Vector2Int(Math.Max(Texture.Width, texture.Width), Texture.Height + texture.Height);
            var expandDelta = newSize - Size;
            expandDelta = (1f * expandDelta / MultipleOfExpand).Ceiling() * MultipleOfExpand;

            Expand(expandDelta);
            if(!Nodes.TryTakeSpace(textureSize, out rect))
                throw new InvalidOperationException("couldn't expand texture");
        }

        Texture.Update(texture.GetPixels(), (int)rect.Left, (int)rect.Top, (int)rect.Width, (int)rect.Height);
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

        var newTexture = Texture.Create(Texture.Width + delta.x, Texture.Height + delta.y).Finish();
        newTexture.Update(Texture.GetPixels(), 0, 0, Texture.Width, Texture.Height);
        Texture = newTexture;

        Nodes.Expand(delta);
    }


    public class TextureMapNode
    {
        protected Rect Rect;
        protected Rect? TakenRect = null;
        protected TextureMapNode? RightChild = null;
        protected TextureMapNode? BottomChild = null;
        protected bool IsFull = false;

        public TextureMapNode(Rect rect)
        {
            Rect = new Rect(rect.Left, rect.Top, (int)rect.Width, (int)rect.Height);
        }

        public bool TryTakeSpace(Vector2Int size, out Rect rect)
        {
            if(IsFull || size.x > Rect.Width || size.y > Rect.Height)
            {
                rect = default;
                return false;
            }

            if(TakenRect == null)
            {
                TakenRect = rect = new Rect(Rect.TopLeft, size);

                if(Rect.Width > size.x)
                    RightChild = new(new Rect(rect.Right, Rect.Top, Rect.Width - size.x, size.y));
                if(Rect.Height > size.y)
                    BottomChild = new(new Rect(Rect.Left, Rect.Top + size.y, Rect.Width, Rect.Height - size.y));

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
                        RightChild = new(new Rect(takenRect.Right, Rect.Top, size.x, takenRect.Height));
                    else
                        RightChild.Expand(size.WithY(0));
                }

                if(BottomChild == null)
                    BottomChild = new(new Rect(Rect.Left, takenRect.Bottom, Rect.Width, size.y));
                else
                    BottomChild.Expand(size);
            }
        }
    }
}
