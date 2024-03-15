using Sandbox;
using Sandcube.Mth;
using System;
using System.Collections.Generic;

namespace Sandcube.Texturing;

public class PathedTextureMap : TextureMap
{
    public readonly string LoadPathPrefix;

    public TextureMapPart Invalid { get; protected set; }
    public TextureMapPart Transparent { get; protected set; }
    public TextureMapPart White { get; protected set; }

    protected Dictionary<string, TextureMapPart> PartsByPath = new();

    public PathedTextureMap(Vector2Int initialSize, Vector2Int multipleOfExpand, string loadPathPrefix = "", int mips = 1, Color32? fillColor = null, int textureBorderSize = 0) :
        base(initialSize, multipleOfExpand, mips, fillColor, textureBorderSize)
    {
        LoadPathPrefix = loadPathPrefix;
        SetupDefaultTextures();
    }

    public PathedTextureMap(Vector2Int initialSize, string loadPathPrefix = "", int mips = 1, Color32? fillColor = null, int textureBorderSize = 0) :
        this(initialSize, new Vector2Int(256, 256), loadPathPrefix, mips, fillColor, textureBorderSize)
    {
    }

    public PathedTextureMap(string loadPathPrefix = "", int mips = 1, Color32? fillColor = null, int textureBorderSize = 0) :
        this(new Vector2Int(256, 256), loadPathPrefix, mips, fillColor, textureBorderSize)
    {
    }

    protected void SetupDefaultTextures()
    {
        Invalid = AddTexture(Texture.Invalid);
        Transparent = AddTexture(Texture.Transparent);
        White = AddTexture(Texture.White);
    }


    public TextureMapPart AddTexture(string path, Texture texture)
    {
        if(PartsByPath.ContainsKey(path))
            throw new InvalidOperationException($"Texture by key {path} was already in a map");

        var textureMapPart = AddTexture(texture);
        PartsByPath.Add(path, textureMapPart);
        return textureMapPart;
    }

    public TextureMapPart AddAnimatedTexture(string path, AnimatedTexture animatedTexture)
    {
        if(PartsByPath.ContainsKey(path))
            throw new InvalidOperationException($"Texture by key {path} was already in a map");

        var textureMapPart = AddAnimatedTexture(animatedTexture);
        PartsByPath.Add(path, textureMapPart);
        return textureMapPart;
    }

    public TextureMapPart UpdateTexture(string path, Texture texture)
    {
        if(!PartsByPath.TryGetValue(path, out var textureMapPart))
            throw new InvalidOperationException($"Texture by key {path} was not presented in a map");

        var rect = textureMapPart.TextureRect;
        if(rect.Size != texture.Size)
            throw new InvalidOperationException($"can't update texture with texture of different size");

        Texture.Update(texture.GetPixels(), (int)rect.Left, (int)rect.Top, (int)rect.Width, (int)rect.Height);
        return textureMapPart;
    }

    public bool HasTexture(string path) => PartsByPath.ContainsKey(path);

    public TextureMapPart GetTexture(string path)
    {
        if(!PartsByPath.TryGetValue(path, out var textureMapPart))
            throw new InvalidOperationException($"Texture by key {path} was not presented in a map");
        return textureMapPart;
    }

    public bool TryGetTexture(string path, out TextureMapPart textureMapPart) =>
        PartsByPath.TryGetValue(path, out textureMapPart);

    public TextureMapPart GetOrAddTexture(string path, Texture texture)
    {
        if(HasTexture(path))
            return GetTexture(path);
        return AddTexture(path, texture);
    }

    public TextureMapPart GetOrLoadTexture(string path, bool warnOnMissing = true) =>
        GetOrLoadTexture(FileSystem.Mounted, path, warnOnMissing);

    public TextureMapPart GetOrLoadTexture(BaseFileSystem fileSystem, string path, bool warnOnMissing = true)
    {
        if(HasTexture(path))
            return GetTexture(path);

        var texture = Texture.Load(fileSystem, LoadPathPrefix + path, warnOnMissing);
        if(texture is null)
            return GameController.Instance!.BlocksTextureMap.Invalid;

        return AddTexture(path, texture);
    }

    public TextureMapPart GetOrLoadAnimatedTexture(string path, Vector2Int atlasSize, float[] framesLength, bool warnOnMissing = true) =>
        GetOrLoadAnimatedTexture(FileSystem.Mounted, path, atlasSize, framesLength, warnOnMissing);

    //TODO: load atlasSize and framesLength from data file
    public TextureMapPart GetOrLoadAnimatedTexture(BaseFileSystem fileSystem, string path, Vector2Int atlasSize, float[] framesLength, bool warnOnMissing = true)
    {
        if(HasTexture(path))
            return GetTexture(path);

        var texture = Texture.Load(fileSystem, LoadPathPrefix + path, warnOnMissing);
        if(texture is null)
            return GameController.Instance!.BlocksTextureMap.Invalid;

        AnimatedTexture animatedTexture = new(texture, atlasSize, framesLength);
        var textureMapPart = AddAnimatedTexture(path, animatedTexture);
        return textureMapPart;
    }
}
