using Sandbox.UI;

namespace Sandcube.UI;

public class AdaptiveTextEntry : TextEntry
{
    public AdaptiveTextEntry()
    {
        Label.Parent = null;
        Label = AddChild<AdaptiveLabel>("content-label");
    }
}
