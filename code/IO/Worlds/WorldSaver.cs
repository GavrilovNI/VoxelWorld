using Sandbox;
using VoxelWorld.Data;
using VoxelWorld.IO.Helpers;
using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.Worlds;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VoxelWorld.IO.Worlds;

public class WorldSaver : Component, ISaver
{
    [Property] private World World { get; set; } = null!;

    protected static GameSaveHelper GameSaveHelper => GameController.Instance!.CurrentGameSaveHelper!;
    protected BaseFileSystem WorldFileSystem => World.WorldFileSystem!;
    protected WorldOptions WorldOptions => World.WorldOptions;


    public virtual async Task<bool> Save()
    {
        ThreadSafe.AssertIsMainThread();

        if(WorldOptions.ChunkSize != World.ChunkSize)
            throw new InvalidOperationException($"Can't save world, saved chunk size {WorldOptions.ChunkSize} is not equal world's chunk size {World.ChunkSize}");

        SaveMarker saveMarker = SaveMarker.NewNotSaved;

        var unsavedChunks = await World.SaveUnsavedChunks(saveMarker);
        var outOfLimitsEntities = World.SaveOutOfLimitsEntitites(saveMarker);
        var unsavedPlayers = World.SaveAllPlayers(saveMarker);

        TaskCompletionSource<bool> taskCompletionSource = new();

        _ = Task.RunInThreadAsync(() =>
        {
            try
            {
                var worldSaveHelper = new WorldSaveHelper(WorldFileSystem);
                var blocksHelper = worldSaveHelper.GetRegionalHelper(WorldSaveHelper.BlocksRegionName, WorldOptions.RegionSize);
                blocksHelper.SaveChunks(unsavedChunks.Where(kv => kv.Value.Blocks is not null)
                    .ToDictionary(kv => kv.Key, kv => kv.Value.Blocks)!);

                var entitiesHelper = worldSaveHelper.GetRegionalHelper(WorldSaveHelper.EntitiesRegionName, WorldOptions.RegionSize);
                entitiesHelper.SaveChunks(unsavedChunks.ToDictionary(kv => kv.Key, kv => (BinaryTag)kv.Value.Entities));

                SavePlayers(GameSaveHelper.PlayersFileSystem, unsavedPlayers);

                SaveOutOfLimitsEntities(worldSaveHelper.FileSystem, outOfLimitsEntities);

                saveMarker.MarkSaved();
                taskCompletionSource.SetResult(true);
            }
            catch(Exception ex)
            {
                taskCompletionSource.SetResult(false);
                Log.Error(ex);
                throw;
            }
        });
        return await taskCompletionSource.Task;
    }

    protected virtual void SaveOutOfLimitsEntities(BaseFileSystem fileSystem, BinaryTag tag)
    {
        using var stream = fileSystem.OpenWrite("out_of_limits.entities");
        using var writer = new BinaryWriter(stream);
        tag.Write(writer);
    }

    protected virtual void SavePlayers(BaseFileSystem fileSystem, IReadOnlyDictionary<ulong, BinaryTag> playerTags)
    {
        foreach(var (steamId, tag) in playerTags)
        {
            var fileName = steamId.ToString();
            if(tag.IsDataEmpty)
            {
                fileSystem.DeleteFile(fileName);
                continue;
            }

            using var stream = fileSystem.OpenWrite(steamId.ToString());
            using var writer = new BinaryWriter(stream);
            tag.Write(writer);
        }
    }
}
