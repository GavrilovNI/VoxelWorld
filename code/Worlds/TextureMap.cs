using Sandbox;

namespace Sandcube.Worlds;

public class TextureMap
{
    public Texture Texture { get; private set; } = Texture.Create(1, 1).Finish();

    protected TextureMapNode Nodes;
    protected Vector2 MultipleOfExpand;


    public TextureMap(Vector2 initialSize, Vector2 multipleOfExpand)
    {
        Texture = Texture.Create((int)initialSize.x, (int)initialSize.y).Finish();
        Nodes = new TextureMapNode(new Rect(0, 0, initialSize.x, initialSize.y));
        MultipleOfExpand = multipleOfExpand;
    }

    public TextureMap(Vector2 initialSize) : this(initialSize, new Vector2(256, 256))
    {
    }

    public TextureMap() : this(new Vector2(256, 256))
    {
    }

    public Rect GetUv(Rect textureRect) => new Rect(textureRect.TopLeft / Texture.Size, textureRect.Size / Texture.Size);

    public Rect AddTexture(Texture texture)
    {
        var textureSize = texture.Size;
        if(!Nodes.TryTakeSpace(textureSize, out Rect rect))
        {
            var currentSize = new Vector2(Texture.Width, Texture.Height);
            var newSize = new Vector2(Math.Max(Texture.Width, texture.Width), Texture.Height + texture.Height);
            var expandDelta = newSize - currentSize;

            expandDelta.x = (int)(MathF.Ceiling(1f * expandDelta.x / MultipleOfExpand.x) * MultipleOfExpand.x);
            expandDelta.y = (int)(MathF.Ceiling(1f * expandDelta.y / MultipleOfExpand.y) * MultipleOfExpand.y);

            Expand(expandDelta);
            if(!Nodes.TryTakeSpace(textureSize, out rect))
                throw new InvalidOperationException("couldn't expand texture");
        }

        Texture.Update(texture.GetPixels(), (int)rect.Left, (int)rect.Top, (int)rect.Width, (int)rect.Height);
        return rect;
    }

    protected void Expand(Vector2 delta)
    {
        if(delta.x < 0 || delta.y < 0)
            throw new ArgumentOutOfRangeException(nameof(delta), delta, "expand size can't be negative");

        var newTexture = Texture.Create((int)(Texture.Width + delta.x), (int)(Texture.Height + delta.y)).Finish();
        newTexture.Update(Texture.GetPixels(), 0, 0, Texture.Width, Texture.Height);
        Texture = newTexture;

        Nodes.Expand(new Vector2(delta.x, delta.y));
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
            Rect = rect;
        }

        public bool TryTakeSpace(Vector2 size, out Rect rect)
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

        public void Expand(Vector2 size)
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
