using Sandcube.Mth;
using System;
using System.Reflection;

namespace Sandcube.Sandcube;

[CustomEditor(typeof(Axis))]
[CustomEditor(typeof(AxisDirection))]
[CustomEditor(typeof(Direction))]
public class CustomEnumControllWidget : ControlWidget
{
    protected PopupWidget? _menu;
    protected readonly FieldInfo[] _fields;

    public override bool IsControlButton => true;
    public override bool IsControlHovered => base.IsControlHovered || _menu.IsValid();

    public CustomEnumControllWidget(SerializedProperty property) : base(property)
    {
        Layout = Layout.Row();
        Layout.Spacing = 2;

        Cursor = CursorShape.Finger;

        Layout = Layout.Row();
        Layout.Spacing = 2;

        var fields = property.PropertyType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(f => f.FieldType == property.PropertyType);

        _fields = fields.ToArray();

        var value = SerializedProperty.GetValue<object>();
        if(value is null && _fields.Length > 0)
            SerializedProperty.SetValue(_fields[0].GetValue(null));
    }

    protected virtual object? GetValue(string name)
    {
        return _fields.FirstOrDefault(f => f.Name == name, null)?.GetValue(null);
    }

    protected virtual string? GetName(object value)
    {
        return _fields.FirstOrDefault(f => Object.Equals(f?.GetValue(null), value), null)?.Name;
    }

    protected override void PaintControl()
    {
        var value = SerializedProperty.GetValue<object>();

        var color = IsControlHovered ? Theme.Blue : Theme.ControlText;
        var rect = LocalRect;

        rect = rect.Shrink(8, 0);

        string? name = value == null ? null : GetName(value);
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
        return object.Equals(value, GetValue(name));
    }

    protected virtual void SetValue(string name)
    {
        SerializedProperty.SetValue(GetValue(name));
    }

    protected virtual void OpenMenu()
    {
        _menu = new(this)
        {
            Layout = Layout.Column()
        };

        foreach(var o in _fields.Select(f => f.Name))
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
