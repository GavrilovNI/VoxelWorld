using Sandbox;
using Sandcube.Blocks;
using Sandcube.Blocks.States;
using Sandcube.Items;
using Sandcube.Meshing;
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

            Texture texture = await GetTexture(autoAttribute, block.DefaultBlockState);
            IMeshPart<ComplexVertex> model = SandcubeGame.Instance!.BlockMeshes.GetVisual(block.DefaultBlockState)!;
            model = new UnlimitedMesh<ComplexVertex>.Builder().Add(model).Scale(Vector3.Zero, 0.3f).Build();

            var propertyType = property.PropertyType;
            if(propertyType.IsGenericType)
            {
                Log.Warning($"Couldn't create block item {thisType.FullName}.{property.Name}, generic types are not supported");
                continue;
            }

            object blockItem;
            try
            {
                blockItem = TypeLibrary.Create<Item>(propertyType, new object[] { block!, model, texture, autoAttribute.StackLimit });
            }
            catch(MissingMethodException)
            {
                Log.Warning($"Couldn't create block item {thisType.FullName}.{property.Name}, constructor with args {typeof(Block)} {typeof(Texture)} not found");
                continue;
            }

            property.SetValue(this, blockItem);
        }
    }

    protected virtual async Task<Texture> GetTexture(AutoBlockItemAttribute autoAttribute, BlockState blockState)
    {
        Texture texture;
        if(autoAttribute.UseRawTexture)
        {
            texture = Texture.Load(FileSystem.Mounted, autoAttribute.RawTexturePath, true);
        }
        else
        {
            (var textureMade, texture) = await MakeBlockItemTexture(blockState);
            if(!textureMade)
                Log.Warning($"Couldn't create texture for {nameof(BlockState)} {blockState}");
        }
        return texture;
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
