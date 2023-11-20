using Sandbox;
using Sandcube.Worlds;

namespace Sandcube.Players;

public class SandcubePlayer : BaseComponent
{
    [Property] public World World { get; private set; } = null!;
}
