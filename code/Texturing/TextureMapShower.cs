using Sandbox;

namespace VoxelWorld.Texturing;

public class TextureMapShower : Component
{
    [Property] public ModelRenderer ModelRenderer { get; set; } = null!;
    [Property] public bool Blocks { get; set; } = true;

    protected override void OnUpdate()
    {
        var texture = Blocks ? GameController.Instance?.BlocksTextureMap.Texture : GameController.Instance?.ItemsTextureMap.Texture;
        if(texture is not null)
        {
            ModelRenderer.SceneObject.Attributes.Set("color", texture);

            var gameobject = ModelRenderer.GameObject;
            var scale = gameobject.Transform.Scale;
            gameobject.Transform.Scale = scale.WithY(scale.x * texture.Size.y / texture.Size.x);
        }
    }
}
