using Editor;
using Sandbox;
using VoxelWorld.IO.Worlds;
using VoxelWorld.Worlds;
using System.Collections.Generic;

namespace VoxelWorld;

public class WorldEditorTool : EditorTool<World>
{
    private WorldEditorToolWindow _window = null!;

    public override void OnEnabled()
    {
        _window = new WorldEditorToolWindow();
        AddOverlay(_window, TextFlag.RightBottom, 10);
    }

    public override void OnUpdate()
    {
        _window.ToolUpdate();
    }

    public override void OnSelectionChanged()
    {
        _window.OnSelectionChanged(GetSelectedComponent<World>());
    }
}

public class WorldEditorToolWindow : WidgetWindow
{
    private World? _world = null;
    private World? World => _world.IsValid() && !_world.Scene.IsEditor ? _world : null;

    public WorldEditorToolWindow()
    {
        Icon = "public";
        Layout = Layout.Column();

        var buttonsRow = Layout.AddRow(0, true);

        var height = 720 * 0.1f;
        var buttonHeight = height * 0.8f - ContentMargins.Top;
        var buttons = CreateButons(buttonHeight);

        FixedHeight = 720 * 0.1f;
        MinimumWidth = 1280 * 0.1f;
        Width = (FixedHeight - ContentMargins.Top) * buttons.Count;

        foreach(var button in buttons)
            buttonsRow.Add(button);

    }

    protected List<IconButton> CreateButons(float buttonHeight)
    {
        return new List<IconButton>()
        {
            new("cleaning_services", ClearWorld)
            {
                ToolTip = "Clear",
                FixedHeight = buttonHeight,
                FixedWidth = buttonHeight,
                Background = Color.Transparent,
                IconSize = buttonHeight
            },
            new("save", SaveWorld)
            {
                ToolTip = "Save",
                FixedHeight = buttonHeight,
                FixedWidth = buttonHeight,
                Background = Color.Transparent,
                IconSize = buttonHeight
            },
        };
    }

    protected void ClearWorld()
    {
        World?.Clear();
    }

    protected void SaveWorld()
    {
        World?.Components?.Get<WorldSaver>()?.Save();
    }

    public void ToolUpdate()
    {
    }

    internal void OnSelectionChanged(World world)
    {
        _world = world;
        if(World.IsValid())
            Show();
        else
            Hide();
        WindowTitle = $"World - {world.GameObject.Name}";
    }
}
