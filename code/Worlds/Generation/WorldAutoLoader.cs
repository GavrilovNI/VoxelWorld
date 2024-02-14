using Sandbox;
using Sandcube.Mth;

namespace Sandcube.Worlds.Generation;

public class WorldAutoLoader : Component
{
    [Property] public World? World { get; set; } = null;
    [Property] public Vector3Int Distance { get; set; } = 2;

    protected override void OnUpdate()
    {
        if(!SandcubeGame.IsStarted || World is null)
            return;

        var centralChunkPositrion = World.GetChunkPosition(Transform.Position);

        var start = centralChunkPositrion - Distance;
        var end = centralChunkPositrion + Distance;

        for(int x = start.x; x <= end.x; ++x)
        {
            for(int y = start.y; y <= end.y; ++y)
            {
                for(int z = start.z; z <= end.z; ++z)
                {
                    var chunkPosition = new Vector3Int(x, y, z);
                    _ = World.GetChunkOrLoad(chunkPosition);
                }
            }
        }
    }


}
