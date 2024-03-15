using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace VoxelWorld.UI;

public class AdaptiveTextEntry : TextEntry
{
    private Label? _placeholderLabel;

    public new string Placeholder
    {
        get => _placeholderLabel?.Text ?? string.Empty;
        set
        {
            if(string.IsNullOrEmpty(value))
            {
                _placeholderLabel?.Delete();
                _placeholderLabel = null;
                return;
            }

            if(_placeholderLabel is null)
                _placeholderLabel = AddChild<AdaptiveLabel>("placeholder");

            _placeholderLabel.Text = value;
        }
    }

    public AdaptiveTextEntry()
    {
        Label.Parent = null;
        Label = AddChild<AdaptiveLabel>("content-label");
    }

    public override void SetProperty(string name, string value)
    {
        if(name == "placeholder")
        {
            Placeholder = value;
            return;
        }
        base.SetProperty(name, value);
    }

    public override void Tick()
    {
        base.Tick();
        SetClass("has-placeholder", string.IsNullOrEmpty(Text) && _placeholderLabel != null);
    }
}
