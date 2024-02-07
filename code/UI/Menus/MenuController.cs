using Sandbox;
using Sandcube.Players;
using Sandcube.UI.Inventories.Players;

namespace Sandcube.UI.Menus;

public class MenuController : Component
{
    private static MenuController? _instance = null;
    public static MenuController? Instance
    {
        get
        {
            if(_instance.IsValid())
                return _instance;
            return null;
        }
        private set => _instance = value;
    }
    [Property] public SandcubePlayer Player { get; set; } = null!;
    [Property] public EscapeMenu EscapeMenu { get; set; } = null!;
    [Property] public InventoryUI PlayerInventoryUI { get; set; } = null!;

    public bool IsAnyOpened => CurrentMenu.IsValid() && CurrentMenu.IsOpened;
    public IMenu? CurrentMenu { get; private set; }


    protected override void OnEnabled()
    {
        if(Instance.IsValid() && Instance != this)
        {
            Log.Warning($"{nameof(Scene)} {Scene} has to much instances of {nameof(MenuController)}. Destroying this...");
            Destroy();
            return;
        }

        if(!Scene.IsEditor)
            Instance = this;
    }

    protected override void OnUpdate()
    {
        if(Input.EscapePressed)
        {
            if(IsAnyOpened)
                CloseCurrentMenu();
            else
                OpenMenu(EscapeMenu);
        }
        else if(Input.Pressed("Inventory"))
        {
            if(!IsAnyOpened)
                OpenMenu(PlayerInventoryUI);
            else if(CurrentMenu == PlayerInventoryUI)
                CloseCurrentMenu();
        }

        if(CurrentMenu.IsValid() && CurrentMenu.IsOpened && !CurrentMenu.IsStillValid(Player))
            CloseCurrentMenu();
    }

    protected virtual void CloseCurrentMenu()
    {
        if(CurrentMenu.IsValid() && CurrentMenu.IsOpened)
        {
            CurrentMenu.Close();
            CurrentMenu = null;
        }
    }

    protected virtual void OpenMenu(IMenu menu)
    {
        CloseCurrentMenu();
        if(menu.IsValid() && !menu.IsOpened)
        {
            CurrentMenu = menu;
            CurrentMenu.Open();
        }
    }
}
