using Sandbox;
using Sandcube.Blocks;
using Sandcube.Items;
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
        var autoBlockItemProperties = TypeLibrary.GetType(GetType()).Properties
            .Where(p => p.PropertyType.IsAssignableTo(typeof(Item)) && p.HasAttribute<AutoBlockItemAttribute>() && p.GetValue(this) is null);

        var photoMaker = SandcubeGame.Instance!.BlockPhotoMaker;

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

            BlockItem blockItem;

            var propertyType = property.PropertyType;
            if(!propertyType.IsAssignableTo(typeof(BlockItem)) || propertyType == typeof(BlockItem))
            {
                var id = block.ModedId;
                var itemTexture = Texture.CreateRenderTarget().WithWidth(BlockItemsTextureSize).WithHeight(BlockItemsTextureSize).Create();
                bool made = await photoMaker.TryMakePhoto(block.DefaultBlockState, itemTexture);
                if(!made)
                {
                    itemTexture = Texture.Invalid;
                    Log.Warning($"Couldn't create texture for item {thisType.FullName}.{property.Name} of block with id '{blockModedId}'");
                }
                blockItem = new(block, itemTexture);
            }
            else
            {
                blockItem = TypeLibrary.Create<BlockItem>(propertyType, new object[] { block! });
                if(blockItem is null)
                {
                    Log.Warning($"Couldn't create {propertyType.Name}. Probably constructor with 1 arg of type {nameof(Block)} wasn't found");
                    continue;
                }
            }

            property.SetValue(this, blockItem);
        }
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
