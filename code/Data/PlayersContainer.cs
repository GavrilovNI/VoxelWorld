using Sandbox;
using Sandcube.Players;
using System;
using System.Collections.Generic;

namespace Sandcube.Data;

public class PlayersContainer
{
    private readonly Dictionary<ulong, Player> _loadedPlayers = new();

    public bool HasLoaded(ulong steamId) => TryGetLoaded(steamId, out _);

    public bool TryGetLoaded(ulong steamId, out Player player)
    {
        if(_loadedPlayers.TryGetValue(steamId, out player!) && player.IsValid)
            return true;
        return false;
    }

    public Player GetLoaded(ulong steamId)
    {
        if(!TryGetLoaded(steamId, out var player))
            throw new KeyNotFoundException($"player with steamid {steamId} wasn't loaded");
        return player;
    }

    /*public void AddLoaded(Player player)
    {
        if(HasLoaded())
    }*/

}
