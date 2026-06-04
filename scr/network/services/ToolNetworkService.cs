using Godot;
using System;
using System.Collections.Generic;

public partial class ToolNetworkService : Node
{
    public static ToolNetworkService Instance { get; private set; }

    private SteamMatchManager Match => SteamMatchManager.Instance;
    private NetworkRoot Network => NetworkRoot.Instance;

    private readonly Dictionary<ulong, ToolData> _activeTools = new();
    private Godot.Collections.Array<Player> _players = new();

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
        Match.OnIrrigateVFX -= HandleIrrigateVFX;
        Match.OnProjectileSpawned -= HandleProjectileSpawn;

        foreach(Player p in _players)
            p.WaterFX.OnPlayWaterFX -= Match.BroadcastIrrigateVFX;
    }

    public void Initialize(Godot.Collections.Array<Player> players)
    {
        _initialized = true;
        _players = players;

        Match.OnToolBeginUse += HandleToolBeginUse;
        Match.OnToolEndUse += HandleToolEndUse;
        Match.OnIrrigateVFX += HandleIrrigateVFX;
        Match.OnProjectileSpawned += HandleProjectileSpawn;

        foreach(Player p in _players)
            p.WaterFX.OnPlayWaterFX += Match.BroadcastIrrigateVFX;
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

        Player player = FindPlayerBySteamId(packet.OwnerSteamId, packet.PlayerId);
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

        Player player = FindPlayerBySteamId(packet.OwnerSteamId, packet.PlayerId);
        if (player == null) return;

        if (_activeTools.TryGetValue(packet.OwnerSteamId, out ToolData tool))
        {
            var ctx = new ItemUseContext(player, player.Tile);
            tool.EndUse(ctx);
            _activeTools.Remove(packet.OwnerSteamId);
        }

        GD.Print($"[ToolNetworkService] Remote EndUse for {packet.OwnerSteamId}");
    }

    private void HandleIrrigateVFX(MatchIrrigateVFXPacket packet)
    {
        Player player = FindPlayerBySteamId(packet.OwnerSteamId, packet.PlayerId);
        player.WaterFX?.Play(false);
    }

    private void HandleProjectileSpawn(MatchProjectileSpawnPacket packet)
    {
        if (Network.Lobby.IsHost && packet.OwnerSteamId != Network.Lobby.LocalSteamId)
        {
            SpawnProjectileFromPacket(packet);
            Match.BroadcastProjectileSpawn(
                packet.PlayerId,
                packet.OwnerSteamId,
                packet.Position,
                packet.Direction,
                packet.InheritedVelocity
            );
            return;
        }

        if (!Network.Lobby.IsHost && packet.OwnerSteamId != Network.Lobby.LocalSteamId)
            SpawnProjectileFromPacket(packet);
    }

    private void SpawnProjectileFromPacket(MatchProjectileSpawnPacket packet)
    {
        Player owner = FindPlayerBySteamId(packet.OwnerSteamId, packet.PlayerId);
        if (owner == null) return;

        var stack = owner.Hotbar.GetCurrentStack();
        if (stack?.Data is not WaterGunData gun) return;

        var level = SplitScreenManager.Instance?.LevelNode;
        if (level == null) return;

        var projectile = gun.ProjectileScene.Instantiate<WaterProjectile>();
        level.AddChild(projectile);
        projectile.GlobalPosition = packet.Position;
        projectile.InitializeFromNetwork(packet.Direction, packet.InheritedVelocity);

        GD.Print($"[ToolNetworkService] Spawned remote projectile for {packet.OwnerSteamId}");
    }

    private Player FindPlayerBySteamId(ulong steamId, int playerId)
    {
        foreach (Player p in _players)
            if (p.OwnerSteamId == steamId && p.PlayerId == playerId)
                return p;

        return null;
    }
}