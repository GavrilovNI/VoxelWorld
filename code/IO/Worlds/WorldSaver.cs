using Sandbox;
using VoxelWorld.Data;
using VoxelWorld.Entities;
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
        var unsavedPlayers = World.SaveAllPlayers();

        TaskCompletionSource<bool> taskCompletionSource = new();

        _ = Task.RunInThreadAsync(() =>
        {
            try
            {
                var worldSaveHelper = new WorldSaveHelper(WorldFileSystem);
                var blocksHelper = worldSaveHelper.GetRegionalHelper(WorldSaveHelper.BlocksRegionName, WorldOptions.RegionSize);
                blocksHelper.SaveChunks(unsavedChunks.ToDictionary(kv => kv.Key, kv => kv.Value.Blocks));

                var entitiesHelper = worldSaveHelper.GetRegionalHelper(WorldSaveHelper.EntitiesRegionName, WorldOptions.RegionSize);
                entitiesHelper.SaveChunks(unsavedChunks.ToDictionary(kv => kv.Key, kv => (BinaryTag)kv.Value.Entities));

                SavePlayers(GameSaveHelper.PlayersFileSystem, unsavedPlayers);

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

    protected virtual void SavePlayers(BaseFileSystem fileSystem, IReadOnlyDictionary<ulong, BinaryTag> playerTags)
    {
        foreach(var (steamId, tag) in playerTags)
        {
            using var stream = fileSystem.OpenWrite(steamId.ToString());
            using var writer = new BinaryWriter(stream);
            tag.Write(writer);
        }
    }
}
