using Sandcube.Entities;
using Sandcube.IO;
using Sandcube.Players;
using Sandcube.Registries;
using Sandcube.Worlds;
using System;
using System.IO;

namespace Sandcube.Data;

public class PlayerData : IBinaryWritable, IBinaryStaticReadable<PlayerData>
{
    public ModedId? WorldId;
    public byte[] EntityData;

    public PlayerData(ModedId? worldId, byte[] entityData)
    {
        WorldId = worldId;
        EntityData = entityData;
    }

    public PlayerData(in Player player)
    {
        if(!player.Initialized)
            throw new InvalidOperationException($"player wan't initialized");

        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);
        player.Write(writer);

        if(stream.Length == 0)
        {
            EntityData = Array.Empty<byte>();
            return;
        }

        WorldId = player.World?.Id;
        EntityData = stream.ToArray();
    }

    public Player CreatePlayer(bool enable = true)
    {
        using var stream = new MemoryStream(EntityData);
        using var reader = new BinaryReader(stream);

        World? world = null;
        if(WorldId.HasValue)
            SandcubeGame.Instance!.Worlds.TryGetWorld(WorldId.Value, out world);

        return (Entity.Read(reader, world, enable) as Player)!;
    }

    public void Write(BinaryWriter writer)
    {
        bool hasWorld = WorldId.HasValue;
        writer.Write(hasWorld);
        if(hasWorld)
            writer.Write<ModedId>(WorldId!.Value);

        writer.Write(EntityData.Length);
        writer.Write(EntityData);
    }

    public static PlayerData Read(BinaryReader reader)
    {
        ModedId? worldId = null;
        if(reader.ReadBoolean())
            worldId = ModedId.Read(reader);

        var entityDataLength = reader.ReadInt32();
        var entityData = reader.ReadBytes(entityDataLength);
        return new PlayerData(worldId, entityData);
    }
}
