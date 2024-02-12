using Sandbox;
using Sandcube.Blocks.States;
using System;
using System.Threading.Tasks;

namespace Sandcube.Texturing.Items;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class CustomBlockItemTextureMakerAttribute : Attribute
{
    public readonly Type Type;

    public CustomBlockItemTextureMakerAttribute(Type type)
    {
        if(!type.IsAssignableTo(typeof(BlockItemTextureMaker)))
            throw new ArgumentException($"{type} is not assignable to {typeof(BlockItemTextureMaker)}");
        Type = type;
    }


    public Task<bool> TryMakePhoto(BlockState blockState, Texture texture)
    {
        var blockPhotoMaker = SandcubeGame.Instance!.BlockPhotoMaker;
        var typeDescription = TypeLibrary.GetType(Type);
        var method = typeDescription.GetMethod(nameof(BlockItemTextureMaker.TryMakePhoto));
        var textureMaker = typeDescription.Create<BlockItemTextureMaker>(new object[] { blockPhotoMaker });
        return method.InvokeWithReturn<Task<bool>>(textureMaker, new object[] { blockState, texture });
    }
}
