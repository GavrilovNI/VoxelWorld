using Sandcube.Mth.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sandcube.Texturing;

public class BlockTexturesLoader
{
    public const string SuffixSeparator = "_";

    public const string FrontSuffix = "front";
    public const string BackSuffix = "back";
    public const string LeftSuffix = "left";
    public const string RightSuffix = "right";
    public const string TopSuffix = "top";
    public const string BottomSuffix = "bottom";
    public const string SideSuffix = "side";
    public const string TopBottomSuffix = "top_bottom";

    public static readonly BlockTexturesLoader OneTexture = new(new Dictionary<Direction, string>()
        {
            { Direction.Forward, string.Empty },
            { Direction.Backward, string.Empty },
            { Direction.Left, string.Empty },
            { Direction.Right, string.Empty },
            { Direction.Up, string.Empty },
            { Direction.Down, string.Empty }
        });

    public static readonly BlockTexturesLoader AllSides = new(new Dictionary<Direction, string>()
        {
            { Direction.Forward, FrontSuffix },
            { Direction.Backward, BackSuffix },
            { Direction.Left, LeftSuffix },
            { Direction.Right, RightSuffix },
            { Direction.Up, TopSuffix },
            { Direction.Down, BottomSuffix }
        });

    public static readonly BlockTexturesLoader Pillar = new(new Dictionary<Direction, string>()
        {
            { Direction.Forward, SideSuffix },
            { Direction.Backward, SideSuffix },
            { Direction.Left, SideSuffix },
            { Direction.Right, SideSuffix },
            { Direction.Up, TopSuffix },
            { Direction.Down, BottomSuffix }
        });

    public static readonly BlockTexturesLoader SimplePillar =
        Pillar.With((Direction.Up, TopBottomSuffix), (Direction.Down, TopBottomSuffix));

    public static readonly BlockTexturesLoader BottomDoor = SimplePillar
        .With((Direction.Forward, FrontSuffix), (Direction.Backward, BackSuffix))
        .Rename((d, s) => d.Axis == Axis.Z ? string.Empty : $"bottom_{s}");

    public static readonly BlockTexturesLoader TopDoor = SimplePillar
        .With((Direction.Forward, FrontSuffix), (Direction.Backward, BackSuffix))
        .Rename((d, s) => d.Axis == Axis.Z ? string.Empty : $"top_{s}");


    public IReadOnlyDictionary<Direction, string> Suffixes { get; init; }
    public IReadOnlyDictionary<Direction, string> TexturePaths { get; init; }

    public BlockTexturesLoader(IReadOnlyDictionary<Direction, string> textureSuffixes) :
        this(textureSuffixes, new Dictionary<Direction, string>())
    {
    }

    public BlockTexturesLoader(IReadOnlyDictionary<Direction, string> textureSuffixes, IReadOnlyDictionary<Direction, string> texturePaths)
    {
        Suffixes = textureSuffixes;
        TexturePaths = texturePaths;
    }


    public BlockTexturesLoader Rename(IReadOnlyDictionary<string, string> suffixOldToNewNames)
    {
        var newSuffixes = new Dictionary<Direction, string>(Suffixes);
        foreach(var (direction, suffix) in newSuffixes)
        {
            if(suffixOldToNewNames.TryGetValue(suffix, out var newSuffix))
                newSuffixes[direction] = newSuffix;
        }
        return new BlockTexturesLoader(newSuffixes, TexturePaths);
    }

    public BlockTexturesLoader Rename(params (string oldSuffix, string newSuffix)[] suffixOldToNewNames) =>
        Rename(suffixOldToNewNames.ToDictionary(e => e.oldSuffix, e => e.newSuffix));

    public BlockTexturesLoader Rename(string oldSuffix, string newSuffix) =>
        Rename(new Dictionary<string, string>() { { oldSuffix, newSuffix } });

    public BlockTexturesLoader Rename(Func<Direction, string, string> suffixChanger)
    {
        var newSuffixes = new Dictionary<Direction, string>(Suffixes);
        foreach(var (direction, suffix) in newSuffixes)
            newSuffixes[direction] = suffixChanger(direction, suffix);
        return new BlockTexturesLoader(newSuffixes, TexturePaths);
    }

    public BlockTexturesLoader Rename(Func<string, string> suffixChanger) => Rename((d, s) => suffixChanger(s));


    public BlockTexturesLoader With(IReadOnlyDictionary<Direction, string> suffixes)
    {
        var newSuffixes = new Dictionary<Direction, string>(Suffixes);
        foreach(var (direction, suffix) in suffixes)
            newSuffixes[direction] = suffix;
        return new BlockTexturesLoader(newSuffixes, TexturePaths);
    }

    public BlockTexturesLoader With(params (Direction direction, string suffix)[] suffixes) =>
        With(suffixes.ToDictionary(e => e.direction, e => e.suffix));

    public BlockTexturesLoader With(Direction direction, string suffix)
    {
        var newSuffixes = new Dictionary<Direction, string>(Suffixes)
        {
            [direction] = suffix
        };
        return new BlockTexturesLoader(newSuffixes, TexturePaths);
    }

    public BlockTexturesLoader WithTextures(params (Direction direction, string? texturePath)[] texturePaths) =>
        WithTextures(texturePaths.ToDictionary(e => e.direction, e => e.texturePath));

    public BlockTexturesLoader WithTextures(IReadOnlyDictionary<Direction, string?> texturePaths)
    {
        var newTexturePaths = new Dictionary<Direction, string>(Suffixes);
        foreach(var (direction, texturePath) in texturePaths)
        {
            if(texturePath is null)
                newTexturePaths.Remove(direction);
            else
                newTexturePaths[direction] = texturePath;
        }
        return new BlockTexturesLoader(Suffixes, newTexturePaths);
    }

    public BlockTexturesLoader WithTexture(Direction direction, string? texturePath)
    {
        var newTexturePaths = new Dictionary<Direction, string>(TexturePaths);
        if(texturePath is null)
            newTexturePaths.Remove(direction);
        else
            newTexturePaths[direction] = texturePath;
        return new BlockTexturesLoader(Suffixes, newTexturePaths);
    }


    public Dictionary<Direction, TextureMapPart> LoadTextures(PathedTextureMap textureMap,
        string texturePathPrefix, string textureExtension = "png", bool loadWithoutSuffixIfUnknown = true)
    {
        Dictionary<Direction, TextureMapPart> result = new();
        foreach(var direction in Direction.All)
        {
            if(TexturePaths.TryGetValue(direction, out var texturePath))
            {
                result[direction] = textureMap.GetOrLoadTexture(texturePath);
                continue;
            }

            string suffix;
            if(!Suffixes.TryGetValue(direction, out suffix!))
            {
                if(!loadWithoutSuffixIfUnknown)
                    continue;
                suffix = string.Empty;
            }

            bool addSuffix = suffix != string.Empty;

            var pathBuilder = new StringBuilder(texturePathPrefix);
            if(addSuffix)
                pathBuilder = pathBuilder.Append(SuffixSeparator).Append(suffix);
            pathBuilder = pathBuilder.Append('.').Append(textureExtension);

            result[direction] = textureMap.GetOrLoadTexture(pathBuilder.ToString());
        }
        return result;
    }

    public Dictionary<Direction, IUvProvider> LoadTextureUvs(PathedTextureMap textureMap,
        string texturePathPrefix, string textureExtension = "png", bool loadWithoutSuffixIfUnknown = true)
    {
        return LoadTextures(textureMap, texturePathPrefix, textureExtension, loadWithoutSuffixIfUnknown)
            .ToDictionary(e => e.Key, e => (IUvProvider)e.Value);
    }
}
