using Sandbox;
using System.Collections.Generic;
using VoxelWorld.Mth;

namespace VoxelWorld.Meshing;

public static class ItemFlatModelCreator
{
    public static Model CreateModelFromMap(string texturePath, Material? material = null, float scale = DefaultValues.ItemModelScale, float pixelSize = DefaultValues.FlatItemPixelSize, float thickness = DefaultValues.FlatItemThickness)
    {
        var unlimitedMesh = CreateFromMap(texturePath, pixelSize * scale, thickness * scale);

        material ??= GameController.Instance!.TranslucentItemsMaterial;

        ModelBuilder modelBuilder = new();
        Mesh mesh = new(material);
        unlimitedMesh.CreateBuffersFor(mesh, 0);
        modelBuilder.AddMesh(mesh);
        return modelBuilder.Create();
    }

    public static UnlimitedMesh<ComplexVertex> CreateFromMap(string texturePath, float pixelSize = DefaultValues.FlatItemPixelSize, float thickness = DefaultValues.FlatItemThickness)
    {
        var map = GameController.Instance!.ItemsTextureMap;
        var textureRect = map.GetOrLoadTexture(texturePath).TextureRect;
        var texture = map.Texture;
        return Create(texture, textureRect, pixelSize, thickness);
    }

    public static UnlimitedMesh<ComplexVertex> Create(Texture texture, RectInt textureRect, float pixelSize = DefaultValues.FlatItemPixelSize, float thickness = DefaultValues.FlatItemThickness)
    {
        List<ComplexVertex> vertices = new();
        List<ushort> indices = new();

        Dictionary<(int x, int y), (ushort front, ushort back)> plateIndices = new();
        Dictionary<(int x, int y, Vector3 normal), (ushort front, ushort back)> sideIndices = new();

        for(int x = textureRect.Left; x < textureRect.Right; ++x)
        {
            for(int y = textureRect.Top; y < textureRect.Bottom; ++y)
            {
                bool shouldAddPixel = texture.GetPixel(x, y).a != 0;

                if(!shouldAddPixel)
                    continue;

                var a = getOrAddPlates(x, y);
                var b = getOrAddPlates(x, y + 1);
                var c = getOrAddPlates(x + 1, y + 1);
                var d = getOrAddPlates(x + 1, y);

                indices.Add(a.front);
                indices.Add(b.front);
                indices.Add(c.front);
                indices.Add(c.front);
                indices.Add(d.front);
                indices.Add(a.front);

                indices.Add(c.back);
                indices.Add(b.back);
                indices.Add(a.back);
                indices.Add(a.back);
                indices.Add(d.back);
                indices.Add(c.back);

                var shouldAddPixelRight = x != textureRect.Right - 1 && texture.GetPixel(x + 1, y).a != 0;
                var shouldAddPixelLeft = x != textureRect.Left && texture.GetPixel(x - 1, y).a != 0;
                var shouldAddPixelDown = y != textureRect.Bottom - 1 && texture.GetPixel(x, y + 1).a != 0;
                var shouldAddPixelUp = y != textureRect.Top && texture.GetPixel(x, y - 1).a != 0;

                if(!shouldAddPixelRight)
                {
                    var aa = getOrAddSides(x + 1, y, Vector3.Right);
                    var bb = getOrAddSides(x + 1, y + 1, Vector3.Right);

                    indices.Add(aa.front);
                    indices.Add(bb.front);
                    indices.Add(bb.back);
                    indices.Add(bb.back);
                    indices.Add(aa.back);
                    indices.Add(aa.front);
                }

                if(!shouldAddPixelLeft)
                {
                    var aa = getOrAddSides(x, y, Vector3.Left);
                    var bb = getOrAddSides(x, y + 1, Vector3.Left);

                    indices.Add(aa.back);
                    indices.Add(bb.back);
                    indices.Add(bb.front);
                    indices.Add(bb.front);
                    indices.Add(aa.front);
                    indices.Add(aa.back);
                }

                if(!shouldAddPixelUp)
                {
                    var aa = getOrAddSides(x, y, Vector3.Up);
                    var bb = getOrAddSides(x + 1, y, Vector3.Up);

                    indices.Add(bb.front);
                    indices.Add(bb.back);
                    indices.Add(aa.back);
                    indices.Add(aa.back);
                    indices.Add(aa.front);
                    indices.Add(bb.front);
                }

                if(!shouldAddPixelDown)
                {
                    var aa = getOrAddSides(x, y + 1, Vector3.Down);
                    var bb = getOrAddSides(x + 1, y + 1, Vector3.Down);

                    indices.Add(bb.back);
                    indices.Add(bb.front);
                    indices.Add(aa.front);
                    indices.Add(aa.front);
                    indices.Add(aa.back);
                    indices.Add(bb.back);
                }
            }
        }

        return new UnlimitedMesh<ComplexVertex>(vertices, indices);

        (ushort front, ushort back) getOrAddPlates(int x, int y)
        {
            if(plateIndices.TryGetValue((x, y), out var result))
                return result;

            var indices = addVertices(x, y, Vector3.Backward, Vector3.Forward, Vector3.Up);
            plateIndices[(x, y)] = indices;
            return indices;
        }

        (ushort front, ushort back) getOrAddSides(int x, int y, in Vector3 normal)
        {
            if(sideIndices.TryGetValue((x, y, normal), out var result))
                return result;

            var backUvMove = new Vector2(normal.y, normal.z);
            var indices = addVertices(x, y, normal, normal, Vector3.Backward, backUvMove);
            sideIndices[(x, y, normal)] = indices;
            return indices;
        }

        (ushort front, ushort back) addVertices(int x, int y, in Vector3 frontNormal, in Vector3 backNormal, in Vector3 tangent, in Vector2 moveBackUv = default)
        {
            var positionFront = new Vector3(0f, (textureRect.Right - x) * pixelSize, (textureRect.Bottom - y) * pixelSize);
            var positionBack = new Vector3(thickness, positionFront.y, positionFront.z);

            var uvFront = new Vector4(1f * x / texture.Width, 1f * y / texture.Height, 0f, 0f);
            var uvBack = uvFront + new Vector4(moveBackUv.x / texture.Width, moveBackUv.y / texture.Height, 0f, 0f);

            vertices.Add(new Vertex(positionFront, frontNormal, tangent, uvFront));
            vertices.Add(new Vertex(positionBack, backNormal, tangent, uvBack));

            return ((ushort)(vertices.Count - 2), (ushort)(vertices.Count - 1));
        }
    }
}
