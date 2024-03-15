using Sandbox;
using VoxelWorld.Worlds.Generation;
using System.Threading;
using System.Threading.Tasks;

namespace VoxelWorld.Worlds.Creation;

public class ChunkLandscapeGenerator : ChunkCreationStage
{
    [Property] protected WorldGenerator Generator { get; set; } = null!;

    protected override void OnAwake()
    {
        Generator ??= Components.Get<WorldGenerator>(true);
    }

    public override async Task<bool> TryProcess(Chunk chunk, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.RunInThreadAsync(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var states = Generator!.Generate(chunk.BlockOffset, chunk.Size);
            cancellationToken.ThrowIfCancellationRequested();
            await chunk.SetBlockStates(states, BlockSetFlags.UpdateModel);
            cancellationToken.ThrowIfCancellationRequested();
        });
        return true;
    }
}
