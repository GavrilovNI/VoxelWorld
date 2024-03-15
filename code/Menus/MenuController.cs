using Sandbox;
using VoxelWorld.Entities;
using VoxelWorld.Players;

namespace VoxelWorld.Menus;

public class MenuController : Component, ILocalPlayerInitializable
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
    [Property] public Player? Player { get; set; }
    [Property] public GameObject EscapeScreen { get; set; } = null!;

    public bool IsAnyOpened => CurrentMenu is not null || CurrentScreen is not null;
    public IMenu? CurrentMenu { get; private set; }
    public GameObject? CurrentScreen { get; private set; }
    public bool ShouldDestroyScreenOnClose { get; private set; }
    public bool IsPlayerInventory { get; private set; }

    public void InitializeLocalPlayer(Player player) => Player = player;

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

    protected override void OnStart()
    {
        EscapeScreen.Enabled = false;
    }

    protected override void OnUpdate()
    {
        if(Input.EscapePressed)
        {
            if(IsAnyOpened)
            {
                CloseMenu();
            }
            else
            {
                CloseMenu();
                OpenScreen(EscapeScreen, false);
            }
        }
        else if(Player.IsValid() && Input.Pressed("Inventory"))
        {
            if(!IsAnyOpened)
            {
                Open(Player.CreateInventoryMenu());
                IsPlayerInventory = true;
            }
            else if(IsPlayerInventory)
            {
                CloseMenu();
            }
        }

        if(CurrentMenu is not null)
        {
            bool shouldClose;
            if(Player.IsValid())
                shouldClose = !CurrentMenu.IsStillValid();
            else
                shouldClose = CurrentScreen is not null && CurrentScreen != EscapeScreen;

            if(shouldClose)
                CloseMenu();
        }
    }

    public virtual void Open(IMenu menu)
    {
        if(!Player.IsValid())
            return;
        CloseMenu();
        CurrentMenu = menu;
        OpenScreen(menu.CreateScreen(), true);
    }

    public virtual void CloseMenu()
    {
        CloseScreen();
        CurrentMenu = null;
        IsPlayerInventory = false;
    }

    protected virtual void OpenScreen(GameObject screen, bool shouldDestroyScreenOnClose)
    {
        CloseScreen();
        CurrentScreen = screen;
        CurrentScreen.Enabled = true;
        CurrentScreen.Parent = GameObject;
        ShouldDestroyScreenOnClose = shouldDestroyScreenOnClose;
    }

    protected virtual void CloseScreen()
    {
        if(CurrentScreen is not null)
        {
            if(ShouldDestroyScreenOnClose)
                CurrentScreen.Destroy();
            else
                CurrentScreen.Enabled = false;
        }

        CurrentScreen = null;
    }
}
