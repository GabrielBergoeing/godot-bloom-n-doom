using Godot;
using Steamworks;
using System.Collections.Generic;

public partial class SteamConnectionManager : Node
{
    public static SteamConnectionManager Instance;
    public NetworkRoot Network => NetworkRoot.Instance;

    private readonly Dictionary<
        ulong,
        CSteamID
    > _peers = new();

    public IReadOnlyDictionary<
        ulong,
        CSteamID
    > Peers => _peers;

    public override void _Ready()
    {
        Instance = this;
        GD.Print("[SteamConnectionManager] Ready");
    }

    public void AddPeer(CSteamID steamId)
    {
        ulong id = steamId.m_SteamID;
        if (_peers.ContainsKey(id))
            return;

        _peers[id] = steamId;
        GD.Print($"[SteamConnectionManager] Peer connected: {id}");

        Network.SetOnlineMode(true);
    }

    public void RemovePeer(CSteamID steamId)
    {
        ulong id = steamId.m_SteamID;
        if (!_peers.Remove(id))
            return;

        GD.Print($"[SteamConnectionManager] Peer disconnected: {id}");
    }

    public bool HasPeer(ulong steamId)
    {
        return _peers.ContainsKey(steamId);
    }

    public IEnumerable<CSteamID> GetAllPeers()
    {
        return _peers.Values;
    }

    public void Clear()
    {
        _peers.Clear();
        Network.SetOnlineMode(false);
    }
}