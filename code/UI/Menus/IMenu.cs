using Sandbox;

namespace Sandcube.UI.Menus;

public interface IMenu : IValid
{
    void Open();
    void Close();
}
