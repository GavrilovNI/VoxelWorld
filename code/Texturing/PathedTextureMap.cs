using Sandbox;
using System;
using System.Collections.Generic;

namespace Sandcube.Texturing;

public class PathedTextureMap
{
    protected TextureMap TextureMap = new();

    public Texture Texture => TextureMap.Texture;
    public Vector2 Size => TextureMap.Size;

    public readonly string LoadPathPrefix;

    protected Dictionary<string, TextureMapPart> PartsByPath = new();

    public PathedTextureMap(string loadPathPrefix = "")
    {
        LoadPathPrefix = loadPathPrefix;
    }

    public Texture GetTexture(Rect textureRect) => TextureMap.GetTexture(textureRect);


    public TextureMapPart AddTexture(string path, Texture texture)
    {
        if(PartsByPath.ContainsKey(path))
            throw new InvalidOperationException($"Texture by key {path} was already in a map");

        var textureMapPart = TextureMap.AddTexture(texture);
        PartsByPath.Add(path, textureMapPart);
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
            return TextureMapPart.Invalid;
        return AddTexture(path, texture);
    }
}
