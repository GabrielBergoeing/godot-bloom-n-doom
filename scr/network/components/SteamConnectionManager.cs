using Godot;
using Steamworks;
using System.Collections.Generic;

public partial class SteamConnectionManager : Node
{
    public static SteamConnectionManager Instance;

    private readonly HashSet<CSteamID> _connectedPeers =
        new();

    public IReadOnlyCollection<CSteamID> ConnectedPeers =>
        _connectedPeers;

    public override void _Ready()
    {
        Instance = this;

        GD.Print("[SteamConnectionManager] Ready");
    }

    public void AddPeer(CSteamID steamId)
    {
        if (_connectedPeers.Add(steamId))
        {
            GD.Print(
                $"[SteamConnectionManager] Peer connected: {steamId}"
            );
        }
    }

    public void RemovePeer(CSteamID steamId)
    {
        if (_connectedPeers.Remove(steamId))
        {
            GD.Print(
                $"[SteamConnectionManager] Peer disconnected: {steamId}"
            );
        }
    }

    public bool IsConnected(CSteamID steamId)
    {
        return _connectedPeers.Contains(steamId);
    }
}