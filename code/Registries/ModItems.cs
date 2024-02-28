using Sandbox;
using Sandcube.Blocks;
using Sandcube.Blocks.States;
using Sandcube.Items;
using System;
using System.Linq;
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

            if(!autoAttribute.TryGetModedId(property, out ModedId blockId))
            {
                Log.Warning($"Couldn't create {nameof(ModedId)} to create item {thisType.FullName}.{property.Name}");
                continue;
            }

            var block = blocksRegistry.Get(blockId);
            if(block is null)
            {
                Log.Warning($"Couldn't find block with id '{blockId}' to create item {thisType.FullName}.{property.Name}");
                continue;
            }

            object blockItem;

            Texture texture;
            if(autoAttribute.UseRawTexture)
            {
                texture = Texture.Load(FileSystem.Mounted, autoAttribute.RawTexturePath, true);
            }
            else
            {
                (var textureMade, texture) = await MakeBlockItemTexture(block.DefaultBlockState);
                if(!textureMade)
                    Log.Warning($"Couldn't create texture for item {thisType.FullName}.{property.Name} of block with id '{block.Id}'");
            }

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
        var photoMaker = SandcubeGame.Instance!.BlockPhotoMaker;

        var itemTexture = Texture.CreateRenderTarget().WithWidth(BlockItemsTextureSize)
            .WithHeight(BlockItemsTextureSize).Create();

        bool made = await photoMaker.TryMakePhoto(blockState, itemTexture);
        if(!made)
            itemTexture = Texture.Invalid;

        return (made, itemTexture);
    }

    public override async Task Register(RegistriesContainer registries)
    {
        await AutoCreateBlockItems(registries.GetRegistry<Block>());
        await base.Register(registries);
    }
}
