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

    public bool IsAnyOpened => CurrentMenu.IsValid();
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
            if(CurrentMenu.IsValid())
                CloseMenu();
            else
                OpenMenu(EscapeMenu);
        }
        else if(Input.Pressed("Inventory"))
        {
            if(!CurrentMenu.IsValid())
                OpenMenu(PlayerInventoryUI);
            else if(CurrentMenu == PlayerInventoryUI)
                CloseMenu();
        }

        if(CurrentMenu.IsValid() && !CurrentMenu.StillValid(Player))
            CloseMenu();
    }

    protected virtual void CloseMenu()
    {
        if(CurrentMenu.IsValid())
        {
            CurrentMenu.Close();
            CurrentMenu = null;
        }
    }

    protected virtual void OpenMenu(IMenu menu)
    {
        if(menu.IsValid())
        {
            CurrentMenu = menu;
            CurrentMenu.Open();
        }
    }
}
