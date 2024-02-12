using Sandbox;
using Sandcube.Blocks;
using Sandcube.Blocks.States;
using Sandcube.Items;
using Sandcube.Mth;
using Sandcube.Texturing;
using Sandcube.Texturing.Items;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandcube.Registries;

public class ModItems : ModRegisterables<Item>
{
    public int BlockItemsTextureSize = 256;

    public ModItems()
    {
    }

    protected virtual async Task AutoCreateBlockItems(Registry<Block> blocksRegistry)
    {
        var thisType = GetType();
        var autoBlockItemProperties = TypeLibrary.GetType(thisType).Properties
            .Where(p => p.PropertyType.IsAssignableTo(typeof(Item)) && p.HasAttribute<AutoBlockItemAttribute>() && p.GetValue(this) is null);


        foreach(var property in autoBlockItemProperties)
        {
            if(!property.CanWrite)
            {
                Log.Warning($"Couldn't create item {thisType.FullName}.{property.Name} as set method is not available.");
                continue;
            }

            var autoAttribute = property.GetCustomAttribute<AutoBlockItemAttribute>();
            var blockId = autoAttribute.BlockId ?? MakeBlockId(property.Name);

            ModedId blockModedId;
            try
            {
                blockModedId = new(autoAttribute.ModId, blockId);
            }
            catch(ArgumentException)
            {
                Log.Warning($"Couldn't create block id to create item {thisType.FullName}.{property.Name}. Tried ModId '{autoAttribute.ModId}' and blockId '{blockId}'");
                continue;
            }

            var block = blocksRegistry.Get(blockModedId);
            if(block is null)
            {
                Log.Warning($"Couldn't find block with id '{blockModedId}' to create item {thisType.FullName}.{property.Name}");
                continue;
            }

            object blockItem;

            (var textureMade, var texture) = await MakeBlockItemTexture(block.DefaultBlockState);
            if(!textureMade)
                Log.Warning($"Couldn't create texture for item {thisType.FullName}.{property.Name} of block with id '{block.Id}'");

            var propertyType = property.PropertyType;
            if(propertyType == typeof(BlockItem))
            {
                blockItem = new BlockItem(block, texture);
            }
            else if(propertyType.IsAssignableTo(typeof(BlockItem)))
            {
                blockItem = TypeLibrary.Create<BlockItem>(propertyType, new object[] { block!, texture });
                if(blockItem is null)
                {
                    Log.Warning($"Couldn't create {propertyType.Name}. Probably constructor with 1 arg of type {nameof(Block)} wasn't found");
                    continue;
                }
            }
            else
            {
                throw new InvalidOperationException($"Can't auto create block item of type {propertyType} for property {thisType.FullName}.{property.Name}");
            }

            property.SetValue(this, blockItem);
        }
    }

    protected virtual async Task<(bool, Texture)> MakeBlockItemTexture(BlockState blockState)
    {
        Vector2Int requestedTextureSize = BlockItemsTextureSize;
        Vector2Int makingTextureSize = requestedTextureSize.WithX(requestedTextureSize.x * 7); // TODO: remove when https://github.com/Facepunch/sbox-issues/issues/4479 get fixed

        var itemTexture = Texture.CreateRenderTarget().WithSize(makingTextureSize).Create();

        var textureMakerAttribute = TypeLibrary.GetAttribute<CustomBlockItemTextureMakerAttribute>(blockState.Block.GetType());
        bool made = await textureMakerAttribute.TryMakePhoto(blockState, itemTexture);
        if(!made)
            itemTexture = Texture.Invalid;

        RectInt partRect = new(((makingTextureSize - requestedTextureSize) / 2f).Floor(), requestedTextureSize);
        itemTexture = itemTexture.GetPart(partRect);

        return (made, itemTexture);
    }

    private static string MakeBlockId(string propertyName)
    {
        const char underscore = '_';
        if(propertyName.Contains(underscore))
            return propertyName.ToLower();

        StringBuilder builder = new();

        for(int i = 0; i < propertyName.Length; ++i)
        {
            var c = propertyName[i];
            if(Char.IsUpper(c))
            {
                c = Char.ToLower(c);
                if(i != 0)
                    builder.Append(underscore);
            }
            builder.Append(c);
        }

        return builder.ToString();
    }

    public override async Task Register(Registry<Item> registry)
    {
        await AutoCreateBlockItems(SandcubeGame.Instance!.BlocksRegistry);
        await base.Register(registry);
    }
}
