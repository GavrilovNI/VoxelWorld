using Sandbox;

namespace Sandcube.Texturing;

public class BlocksTextureMapShower : Component
{
    [Property]
    public ModelRenderer ModelRenderer { get; set; } = null!;

    protected override void OnUpdate()
    {
        var texture = GameController.Instance?.BlocksTextureMap.Texture;
        if(texture is not null)
        {
            ModelRenderer.SceneObject.Attributes.Set("color", texture);

            var gameobject = ModelRenderer.GameObject;
            var scale = gameobject.Transform.Scale;
            gameobject.Transform.Scale = scale.WithY(scale.x * texture.Size.y / texture.Size.x);
        }
    }
}
