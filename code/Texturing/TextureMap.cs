using Sandbox;
using Sandcube.Mth;
using System;

namespace Sandcube.Texturing;

public class TextureMap
{
    public Texture Texture { get; private set; } = Texture.Create(1, 1).Finish();

    protected TextureMapNode Nodes;
    protected Vector2Int MultipleOfExpand;

    public Vector2Int Size => (Vector2Int)Texture.Size;

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

    public Rect GetUv(Rect textureRect) => new Rect(textureRect.TopLeft / Texture.Size, textureRect.Size / Texture.Size);
    public Texture GetTexture(Rect textureRect)
    {
        var result = Texture.Create((int)textureRect.Width, (int)textureRect.Height).Finish();
        Color32[] data = new Color32[(int)textureRect.Width * (int)textureRect.Height];
        Texture.GetPixels<Color32>(((int)textureRect.Left, (int)textureRect.Top, (int)textureRect.Width, (int)textureRect.Height), 0, 0, data, ImageFormat.RGBA8888);
        result.Update(data, 0, 0, (int)textureRect.Width, (int)textureRect.Height);
        return result;
    }

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
