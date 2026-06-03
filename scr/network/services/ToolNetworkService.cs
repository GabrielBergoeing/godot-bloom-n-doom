using Godot;
using System;
using System.Collections.Generic;

public partial class ToolNetworkService : Node
{
    public static ToolNetworkService Instance { get; private set; }

    private SteamMatchManager Match => SteamMatchManager.Instance;
    private NetworkRoot Network => NetworkRoot.Instance;

    private readonly Dictionary<ulong, ToolData> _activeTools = new();

    private bool _initialized = false;

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        if (!_initialized || Match == null) return;
        Match.OnToolBeginUse -= HandleToolBeginUse;
        Match.OnToolEndUse -= HandleToolEndUse;
    }

    public void Initialize()
    {
        _initialized = true;
        Match.OnToolBeginUse += HandleToolBeginUse;
        Match.OnToolEndUse += HandleToolEndUse;
        GD.Print($"[ToolNetworkService] Initialized — IsHost: {Network.Lobby.IsHost}");
    }

    public void UpdateRemoteToolTransform(Player player)
    {
        if (!_activeTools.TryGetValue(player.OwnerSteamId, out ToolData tool)) return;

        var ctx = new ItemUseContext(player, player.Tile);
        tool.TickUse(ctx, 0);
    }

    private void HandleToolBeginUse(MatchToolBeginUsePacket packet)
    {
        if (packet.OwnerSteamId == Network.Lobby.LocalSteamId) return;

        Player player = FindPlayerBySteamId(packet.OwnerSteamId);
        if (player == null)
        {
            GD.PrintErr($"[ToolNetworkService] BeginUse: player not found {packet.OwnerSteamId}");
            return;
        }

        var stack = player.Hotbar.GetStackAt(packet.ItemSlot);
        if (stack?.Data is not ToolData tool)
        {
            GD.PrintErr($"[ToolNetworkService] BeginUse: no tool at slot {packet.ItemSlot}");
            return;
        }

        var ctx = new ItemUseContext(player, player.Tile);
        tool.BeginUse(ctx);
        _activeTools[packet.OwnerSteamId] = tool;

        GD.Print($"[ToolNetworkService] Remote BeginUse for {packet.OwnerSteamId}");
    }

    private void HandleToolEndUse(MatchToolEndUsePacket packet)
    {
        if (packet.OwnerSteamId == Network.Lobby.LocalSteamId) return;

        Player player = FindPlayerBySteamId(packet.OwnerSteamId);
        if (player == null) return;

        if (_activeTools.TryGetValue(packet.OwnerSteamId, out ToolData tool))
        {
            var ctx = new ItemUseContext(player, player.Tile);
            tool.EndUse(ctx);
            _activeTools.Remove(packet.OwnerSteamId);
        }

        GD.Print($"[ToolNetworkService] Remote EndUse for {packet.OwnerSteamId}");
    }

    private Player FindPlayerBySteamId(ulong steamId)
    {
        if (PickupNetworkService.Instance == null) return null;

        foreach (var node in GetTree().GetNodesInGroup("players"))
            if (node is Player p && p.OwnerSteamId == steamId)
                return p;

        return null;
    }
}