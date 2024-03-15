using Sandbox;
using System;

namespace VoxelWorld.UI.Menus.MainMenus;

public class MainMenuController : Component
{
    [Property] protected MainMenu MainMenu { get; set; } = null!;

    protected PanelComponent CurrentMenu { get; set; } = null!;

    protected override void OnEnabled()
    {
        var menus = Components.GetAll<PanelComponent>();
        foreach(var menu in menus)
        {
            if(menu == MainMenu)
                continue;
            menu.Enabled = false;
        }
        OpenMenu(MainMenu);
    }

    public void OpenMenu(PanelComponent menu)
    {
        ArgumentNullException.ThrowIfNull(menu);

        if(CurrentMenu is not null)
        {
            if(CurrentMenu == menu)
                return;
            CurrentMenu.Enabled = false;
        }
        CurrentMenu = menu;
        CurrentMenu.Enabled = true;
    }

    public void CloseMenu(PanelComponent menu)
    {
        ArgumentNullException.ThrowIfNull(menu);

        if(CurrentMenu == MainMenu)
            return;
        if(CurrentMenu != menu)
            return;

        OpenMenu(MainMenu);
    }

    public void CloseCurrentMenu() => CloseMenu(CurrentMenu);
}
