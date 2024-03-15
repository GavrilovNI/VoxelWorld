using Editor;
using Sandbox;
using VoxelWorld.Mth.Enums;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VoxelWorld.Editor;

[CustomEditor(typeof(CustomEnum))]
public class CustomEnumControllWidget : ControlWidget
{
    protected PopupWidget? _menu;
    protected readonly MethodInfo _tryParseMethod;
    protected readonly IEnumerable<CustomEnum> _enumValues;

    public override bool IsControlButton => true;
    public override bool IsControlHovered => base.IsControlHovered || _menu.IsValid();

    public CustomEnumControllWidget(SerializedProperty property) : base(property)
    {
        Layout = Layout.Row();
        Layout.Spacing = 2;

        Cursor = CursorShape.Finger;

        Layout = Layout.Row();
        Layout.Spacing = 2;

        _tryParseMethod = property.PropertyType.GetMethod("TryParse")!;
        _enumValues = (property.PropertyType.GetProperty("All", BindingFlags.Static | BindingFlags.Public)!.GetValue(null) as IEnumerable)!.Cast<CustomEnum>();

        var value = SerializedProperty.GetValue<object>();
        if(value is null)
            SerializedProperty.SetValue(_enumValues.First());
    }

    protected virtual CustomEnum Parse(string name)
    {
        object?[] parameters = new object?[] { name, null };
        _tryParseMethod.Invoke(null, parameters);
        return (parameters[1] as CustomEnum)!;
    }

    protected override void PaintControl()
    {
        var value = SerializedProperty.GetValue<CustomEnum>();

        var color = IsControlHovered ? Theme.Blue : Theme.ControlText;
        var rect = LocalRect;

        rect = rect.Shrink(8, 0);

        string? name = value == null ? null : value.Name;
        Paint.SetPen(color);
        Paint.DrawText(rect, name ?? "Unset", TextFlag.LeftCenter);

        Paint.SetPen(color);
        Paint.DrawIcon(rect, "Arrow_Drop_Down", 17, TextFlag.RightCenter);
    }

    protected override void OnMousePress(MouseEvent e)
    {
        base.OnMousePress(e);

        if(e.LeftMouseButton)
        {
            if(_menu.IsValid())
                _menu.Close();
            else
                OpenMenu();
        }
    }

    protected virtual bool IsCurrent(string name)
    {
        var value = SerializedProperty.GetValue<object>();
        return object.Equals(value, Parse(name));
    }

    protected virtual void SetValue(string name)
    {
        SerializedProperty.SetValue(Parse(name));
    }

    protected virtual void OpenMenu()
    {
        _menu = new(this)
        {
            Layout = Layout.Column()
        };

        foreach(var o in _enumValues.Select(v => v.Name))
        {
            var button = _menu.Layout.Add(new Widget(_menu));
            button.MinimumSize = 22;
            button.MouseLeftPress = () => { SetValue(o); _menu.Update(); };
            button.OnPaintOverride = () => PaintOption(button.LocalRect, o);
        }

        _menu.OpenAt(ScreenRect.BottomLeft, animateOffset: new Vector2(0, 8));
        _menu.MinimumWidth = ScreenRect.Width;
    }

    protected virtual bool PaintOption(Rect rect, string e)
    {
        var active = IsCurrent(e);

        if(Paint.HasMouseOver)
        {
            Paint.ClearPen();
            Paint.SetBrush(Theme.Blue.WithAlpha(0.5f));
            Paint.DrawRect(rect.Shrink(3, 0), 0);
            Paint.SetPen(Theme.White);
        }
        else
        {
            Paint.SetPen(active ? Theme.White : Theme.ControlText);
        }

        rect = rect.Shrink(8, 0);

        if(active)
            Paint.DrawIcon(rect, "check", 13, TextFlag.LeftCenter);

        rect = rect.Shrink(24, 0, 0, 0);

        Paint.SetDefaultFont(8, active ? 1000 : 400);
        Paint.DrawText(rect, e, TextFlag.LeftCenter);
        return true;
    }
}
