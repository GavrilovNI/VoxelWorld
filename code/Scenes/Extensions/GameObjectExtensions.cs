

namespace Sandcube.Scenes.Extensions;

public static class GameObjectExtensions
{
    public static T AddComponent<T>(this GameObject gameObject, T component, bool enabled = true) where T : BaseComponent
    {
        component.GameObject = gameObject;
        gameObject.Components.Add(component);

        component.InitializeComponent();
        component.Enabled = enabled;
        return component;
    }
}
