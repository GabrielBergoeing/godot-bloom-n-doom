using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using Steamworks;

public partial class SteamMatchManager : Node
{
    public static SteamMatchManager Instance;
    public NetworkRoot Network => NetworkRoot.Instance;
    public GameManager Game => GameManager.Instance;

    public ulong LocalSteamId => Network.Lobby.LocalSteamId;
    public bool IsHost => Network.Lobby.IsHost;

    public event Action<MatchStartPacket> OnMatchStarted;
    public event Action<MatchPlayerTransformPacket> OnPlayerTransformReceived;
    public event Action<float> OnTimerSyncReceived;

    public event Action<MatchPlayerHotbarPacket> OnHotbarSyncReceived;
    public event Action<MatchPickupSpawnedPacket> OnPickupSpawned;
    public event Action<MatchPickupCollectedPacket> OnPickupCollected;
    public event Action<MatchPickupSpawnRequestPacket> OnPickupSpawnRequested;
    public event Action<MatchPickupCollectRequestPacket> OnPickupCollectRequested;

    public event Action<MatchFarmPreparePacket> OnFarmPrepared;
    public event Action<MatchFarmPlantPacket> OnFarmPlanted;
    public event Action<MatchFarmRemovePacket> OnFarmRemoved;
    public event Action<MatchFarmIrrigatePacket> OnFarmIrrigated;
    public event Action<MatchFarmFertilizePacket> OnFarmFertilized;
    public event Action<MatchFarmSabotagePacket> OnFarmSabotaged;

    public event Action<MatchToolBeginUsePacket> OnToolBeginUse;
    public event Action<MatchToolEndUsePacket> OnToolEndUse;
    public event Action<MatchPlantIgnitePacket> OnPlantIgnited;
    public event Action<MatchIrrigateVFXPacket> OnIrrigateVFX;
    public event Action<MatchProjectileSpawnPacket> OnProjectileSpawned;


    // PlayerId -> remote transform target position
    private readonly Dictionary<int, Vector2> _remotePositions = new();

    private bool _matchSceneReady = false;
    private MatchStartPacket _pendingMatchPacket = null;

    public override void _Ready()
    {
        Instance = this;
    }

    public void Initialize(SteamPacketRouter router)
    {
        router.RegisterHandler((byte)NetworkPacketType.MatchPlayerTransform, HandlePlayerTransform);
        router.RegisterHandler((byte)NetworkPacketType.MatchStart, HandleMatchStart);
        router.RegisterHandler((byte)NetworkPacketType.MatchTimerSync, HandleTimerSync);

        router.RegisterHandler((byte)NetworkPacketType.MatchPlayerHotbar, HandleHotbarSync);
        router.RegisterHandler((byte)NetworkPacketType.MatchPickupSpawned, HandlePickupSpawned);
        router.RegisterHandler((byte)NetworkPacketType.MatchPickupCollected, HandlePickupCollected);
        router.RegisterHandler((byte)NetworkPacketType.MatchPickupSpawnRequest, HandlePickupSpawnRequest);
        router.RegisterHandler((byte)NetworkPacketType.MatchPickupCollectRequest, HandlePickupCollectRequest);

        router.RegisterHandler((byte)NetworkPacketType.MatchFarmPrepare, HandleFarmPrepare);
        router.RegisterHandler((byte)NetworkPacketType.MatchFarmPlant, HandleFarmPlant);
        router.RegisterHandler((byte)NetworkPacketType.MatchFarmRemove, HandleFarmRemove);
        router.RegisterHandler((byte)NetworkPacketType.MatchFarmIrrigate, HandleFarmIrrigate);
        router.RegisterHandler((byte)NetworkPacketType.MatchFarmFertilize, HandleFarmFertilize);
        router.RegisterHandler((byte)NetworkPacketType.MatchFarmSabotage, HandleFarmSabotage);

        router.RegisterHandler((byte)NetworkPacketType.MatchToolBeginUse, HandleToolBeginUse);
        router.RegisterHandler((byte)NetworkPacketType.MatchToolEndUse, HandleToolEndUse);
        router.RegisterHandler((byte)NetworkPacketType.MatchPlantIgnite, HandlePlantIgnite);
        router.RegisterHandler((byte)NetworkPacketType.MatchIrrigateVFX, HandleIrrigateVFX);
        router.RegisterHandler((byte)NetworkPacketType.MatchProjectileSpawn, HandleProjectileSpawn);
    }

    public void BroadcastMatchStart()
    {
        if (!IsHost) return;

        MatchStartPacket packet = BuildMatchStartPacket();
        GD.Print($"[SteamMatchManager] Broadcasting match start with {packet.Players.Count} players");

        Network.Lobby.Broadcast(packet);
        Network.LobbyService.Clear();

        Callable.From(() => OnMatchStarted?.Invoke(packet)).CallDeferred();
        GD.Print("[SteamMatchManager] Local OnMatchStarted invoke");
    }

    public void BroadcastTransform(int playerId, ulong ownerSteamId, Vector2 position, float rotation, string action, Vector2 facingDir)
    {
        MatchPlayerTransformPacket packet = new()
        {
            PlayerId = playerId,
            OwnerSteamId = ownerSteamId,
            Position = position,
            Rotation = rotation,
            Action = action,
            FacingDir = facingDir
        };
        Network.Lobby.Broadcast(packet);
    }

    public void BroadcastTimerSync(float timeRemaining)
    {
        MatchTimerSyncPacket packet = new() { TimeRemaining = timeRemaining };
        Network.Lobby.Broadcast(packet);
    }

    public void BroadcastHotbarSlot(ulong ownerSteamId, int slotIndex, string itemId, int amount)
    {
        MatchPlayerHotbarPacket packet = new()
        {
            OwnerSteamId = ownerSteamId,
            SlotIndex = slotIndex,
            ItemId = itemId,
            Amount = amount
        };
        Network.Lobby.Broadcast(packet);
    }

    public void BroadcastPickupSpawned(string itemId, Vector2 position, int networkPickupId)
    {
        MatchPickupSpawnedPacket packet = new()
        {
            ItemId = itemId,
            Position = position,
            NetworkPickupId = networkPickupId
        };
        Network.Lobby.Broadcast(packet);
    }

    public void BroadcastPickupCollected(int networkPickupId)
    {
        MatchPickupCollectedPacket packet = new()
        {
            NetworkPickupId = networkPickupId
        };
        Network.Lobby.Broadcast(packet);
    }

    public void RequestPickupSpawn(ulong requesterSteamId, string itemId, Vector2 position)
    {
        MatchPickupSpawnRequestPacket packet = new()
        {
            RequesterSteamId = requesterSteamId,
            ItemId = itemId,
            Position = position
        };
        Network.Lobby.Broadcast(packet);
    }

    public void RequestPickupCollect(int networkPickupId, ulong requesterSteamId)
    {
        MatchPickupCollectRequestPacket packet = new()
        {
            NetworkPickupId = networkPickupId,
            RequesterSteamId = requesterSteamId
        };
        Network.Lobby.Broadcast(packet);
    }

    public void BroadcastFarmPrepared(Vector2I cell)
    {
        MatchFarmPreparePacket packet = new()
        {
            Cell = cell
        };
        Network.Lobby.Broadcast(packet);
    }

    public void BroadcastFarmPlanted(Vector2I cell, int playerIndex, string seedId)
    {
        MatchFarmPlantPacket packet = new()
        {
            Cell = cell,
            PlayerIndex = playerIndex,
            SeedId = seedId
        };
        Network.Lobby.Broadcast(packet);
    }

    public void BroadcastFarmRemoved(Vector2I cell, int playerIndex)
    {
        MatchFarmRemovePacket packet = new()
        {
            Cell = cell,
            PlayerIndex = playerIndex
        };
        Network.Lobby.Broadcast(packet);
    }

    public void BroadcastFarmIrrigated(Vector2I cell)
    {
        MatchFarmIrrigatePacket packet = new()
        {
            Cell = cell
        };
        Network.Lobby.Broadcast(packet);
    }

    public void BroadcastFarmFertilized(Vector2I cell)
    {
        MatchFarmFertilizePacket packet = new()
        {
            Cell = cell
        };
        Network.Lobby.Broadcast(packet);
    }

    public void BroadcastFarmSabotaged(Vector2I cell, int playerIndex)
    {
        MatchFarmSabotagePacket packet = new()
        {
            Cell = cell,
            PlayerIndex = playerIndex
        };

        Network.Lobby.Broadcast(packet);
    }

    public void BroadcastToolBeginUse(int playerId, int itemSlot, ulong ownerSteamId)
    {
        MatchToolBeginUsePacket packet = new()
        {
            PlayerId = playerId,
            ItemSlot = itemSlot,
            OwnerSteamId = ownerSteamId
        };
        Network.Lobby.Broadcast(packet);
    }

    public void BroadcastToolEndUse(int playerId, int itemSlot, ulong ownerSteamId)
    {
        MatchToolEndUsePacket packet = new()
        {
            PlayerId = playerId,
            ItemSlot = itemSlot,
            OwnerSteamId = ownerSteamId
        };
        Network.Lobby.Broadcast(packet);
    }

    public void BroadcastPlantIgnite(Vector2I cell)
    {
        MatchPlantIgnitePacket packet = new() { Cell = cell };
        Network.Lobby.Broadcast(packet);
    }

    public void BroadcastIrrigateVFX(Player p)
    {
        MatchIrrigateVFXPacket packet = new()         
        {
            PlayerId = p.PlayerId,
            OwnerSteamId = p.OwnerSteamId
        };
        Network.Lobby.Broadcast(packet);
    }

    public void BroadcastProjectileSpawn(int playerId, ulong ownerSteamId, Vector2 position, Vector2 direction, Vector2 inheritedVelocity)
    {
        MatchProjectileSpawnPacket packet = new()
        {
            PlayerId = playerId,
            OwnerSteamId = ownerSteamId,
            Position = position,
            Direction = direction,
            InheritedVelocity = inheritedVelocity
        };
        Network.Lobby.Broadcast(packet);
    }

    public Vector2? GetRemotePosition(int playerId)
    {
        return _remotePositions.TryGetValue(playerId, out var pos) ? pos : null;
    }

    private MatchStartPacket BuildMatchStartPacket()
    {
        GD.Print($"[SteamMatchManager] Building packet — LocalPlayers: {Game.LobbyPlayers.Count}, RemoteStates: {LobbyStateService.Instance.RemoteStates.Count}");

        MatchStartPacket packet = new();
        int spawnIndex = 0;

        // Local players — SteamId must be set by this point via OnPlayerJoined
        foreach (var player in Game.LobbyPlayers)
        {
            if (player.SteamId == 0)
                GD.PrintErr($"[SteamMatchManager] WARNING: Player {player.PlayerId} has SteamId 0 — ownership will break");

            GD.Print($"[SteamMatchManager] Adding local -> PlayerId: {player.PlayerId}, SteamId: {player.SteamId}, Char: {player.CharacterIndex}, Spawn: {spawnIndex}");

            packet.Players.Add(new PlayerSpawnData
            {
                SteamId = player.SteamId,
                PlayerId = spawnIndex,
                CharacterIndex = player.CharacterIndex,
                SpawnIndex = spawnIndex,
                IsLocalOwner = true
            });
            spawnIndex++;
        }

        var remoteSource = LobbyStateService.Instance.RemoteStates.Count > 0
            ? GetRemoteFromLobbyService()
            : GetRemoteFromLobbyManager();

        foreach (var kvp in remoteSource)
        {
            packet.Players.Add(new PlayerSpawnData
            {
                SteamId = kvp.steamId,
                PlayerId = spawnIndex,
                CharacterIndex = kvp.charIndex,
                SpawnIndex = spawnIndex,
                IsLocalOwner = false
            });
            spawnIndex++;
        }

        GD.Print($"[SteamMatchManager] Packet complete ({packet.Players.Count} players)");
        return packet;
    }

    // Call this from SplitScreenManager after subscribing
    public void NotifyMatchSceneReady()
    {
        _matchSceneReady = true;
        GD.Print("[SteamMatchManager] Match scene ready");

        if (_pendingMatchPacket != null)
        {
            GD.Print("[SteamMatchManager] Flushing pending match start packet");
            OnMatchStarted?.Invoke(_pendingMatchPacket);
            _pendingMatchPacket = null;
        }
    }

    public void ResetMatchScene()
    {
        _matchSceneReady = false;
        _pendingMatchPacket = null;
    }

    private IEnumerable<(ulong steamId, int charIndex, int playerId)> GetRemoteFromLobbyService()
    {
        foreach (var kvp in LobbyStateService.Instance.RemoteStates)
            yield return (kvp.Key.steamId, kvp.Value.CharacterIndex, kvp.Value.PlayerId);
    }

    private IEnumerable<(ulong steamId, int charIndex, int playerId)> GetRemoteFromLobbyManager()
    {
        foreach (var kvp in Network.Lobby.Players)
        {
            if (kvp.SteamId == LocalSteamId) continue;
            yield return (kvp.SteamId, kvp.CharacterIndex, kvp.PlayerId);
        }
    }

    private void HandleMatchStart(CSteamID sender, byte[] data)
    {
        GD.Print($"[SteamMatchManager] HandleMatchStart from {sender.m_SteamID}");
        PacketReader reader = new PacketReader(data);
        reader.ReadByte();

        MatchStartPacket packet = new();
        packet.Deserialize(reader);

        ulong localSteamId = Network.Lobby.LocalSteamId;
        foreach (var p in packet.Players)
        {
            p.IsLocalOwner = p.SteamId == localSteamId;
            GD.Print($"[SteamMatchManager] Player {p.PlayerId} SteamId:{p.SteamId} IsLocal:{p.IsLocalOwner}");
        }

        GD.Print($"[SteamMatchManager] Match start received, {packet.Players.Count} players");

        Callable.From(() =>
        {
            if (_matchSceneReady)
                OnMatchStarted?.Invoke(packet);
            else
            {
                GD.Print("[SteamMatchManager] Match scene not ready, queuing packet");
                _pendingMatchPacket = packet;
            }
        }).CallDeferred();
    }

    private void HandlePlayerTransform(CSteamID sender, byte[] data)
    {
        PacketReader reader = new PacketReader(data);
        reader.ReadByte();

        MatchPlayerTransformPacket packet = new();
        packet.Deserialize(reader);

        _remotePositions[packet.PlayerId] = packet.Position;

        Callable.From(() => OnPlayerTransformReceived?.Invoke(packet)).CallDeferred();
    }

    private void HandleTimerSync(CSteamID sender, byte[] data)
    {
        PacketReader reader = new PacketReader(data);
        reader.ReadByte();
        MatchTimerSyncPacket packet = new();
        packet.Deserialize(reader);
        Callable.From(() => OnTimerSyncReceived?.Invoke(packet.TimeRemaining)).CallDeferred();
    }

    private void HandlePickupSpawned(CSteamID sender, byte[] data)
    {
        var reader = new PacketReader(data);
        reader.ReadByte();
        var packet = new MatchPickupSpawnedPacket();
        packet.Deserialize(reader);
        Callable.From(() => OnPickupSpawned?.Invoke(packet)).CallDeferred();
    }

    private void HandlePickupCollected(CSteamID sender, byte[] data)
    {
        var reader = new PacketReader(data);
        reader.ReadByte();
        var packet = new MatchPickupCollectedPacket();
        packet.Deserialize(reader);
        Callable.From(() => OnPickupCollected?.Invoke(packet)).CallDeferred();
    }

    private void HandleHotbarSync(CSteamID sender, byte[] data)
    {
        var reader = new PacketReader(data);
        reader.ReadByte();
        var packet = new MatchPlayerHotbarPacket();
        packet.Deserialize(reader);
        Callable.From(() => OnHotbarSyncReceived?.Invoke(packet)).CallDeferred();
    }

    private void HandlePickupSpawnRequest(CSteamID sender, byte[] data)
    {
        var reader = new PacketReader(data);
        reader.ReadByte();
        var packet = new MatchPickupSpawnRequestPacket();
        packet.Deserialize(reader);
        Callable.From(() => OnPickupSpawnRequested?.Invoke(packet)).CallDeferred();
    }

    private void HandlePickupCollectRequest(CSteamID sender, byte[] data)
    {
        var reader = new PacketReader(data);
        reader.ReadByte();
        var packet = new MatchPickupCollectRequestPacket();
        packet.Deserialize(reader);
        Callable.From(() => OnPickupCollectRequested?.Invoke(packet)).CallDeferred();
    }

    private void HandleFarmPrepare(CSteamID sender, byte[] data)
    {
        var reader = new PacketReader(data);
        reader.ReadByte();
        var packet = new MatchFarmPreparePacket();
        packet.Deserialize(reader);
        Callable.From(() => OnFarmPrepared?.Invoke(packet)).CallDeferred();
    }

    private void HandleFarmPlant(CSteamID sender, byte[] data)
    {
        var reader = new PacketReader(data);
        reader.ReadByte();
        var packet = new MatchFarmPlantPacket();
        packet.Deserialize(reader);
        Callable.From(() => OnFarmPlanted?.Invoke(packet)).CallDeferred();
    }

    private void HandleFarmRemove(CSteamID sender, byte[] data)
    {
        var reader = new PacketReader(data);
        reader.ReadByte();
        var packet = new MatchFarmRemovePacket();
        packet.Deserialize(reader);
        Callable.From(() => OnFarmRemoved?.Invoke(packet)).CallDeferred();
    }

    private void HandleFarmIrrigate(CSteamID sender, byte[] data)
    {
        var reader = new PacketReader(data);
        reader.ReadByte();
        var packet = new MatchFarmIrrigatePacket();
        packet.Deserialize(reader);
        Callable.From(() => OnFarmIrrigated?.Invoke(packet)).CallDeferred();
    }

    private void HandleFarmFertilize(CSteamID sender, byte[] data)
    {
        var reader = new PacketReader(data);
        reader.ReadByte();
        var packet = new MatchFarmFertilizePacket();
        packet.Deserialize(reader);
        Callable.From(() => OnFarmFertilized?.Invoke(packet)).CallDeferred();
    }

    private void HandleFarmSabotage(CSteamID sender, byte[] data)
    {
        var reader = new PacketReader(data);
        reader.ReadByte();
        var packet = new MatchFarmSabotagePacket();
        packet.Deserialize(reader);
        Callable.From(() => OnFarmSabotaged?.Invoke(packet)).CallDeferred();
    }

    private void HandleToolBeginUse(CSteamID sender, byte[] data)
    {
        var reader = new PacketReader(data);
        reader.ReadByte();
        var packet = new MatchToolBeginUsePacket();
        packet.Deserialize(reader);
        Callable.From(() => OnToolBeginUse?.Invoke(packet)).CallDeferred();
    }

    private void HandleToolEndUse(CSteamID sender, byte[] data)
    {
        var reader = new PacketReader(data);
        reader.ReadByte();
        var packet = new MatchToolEndUsePacket();
        packet.Deserialize(reader);
        Callable.From(() => OnToolEndUse?.Invoke(packet)).CallDeferred();
    }

    private void HandlePlantIgnite(CSteamID sender, byte[] data)
    {
        var reader = new PacketReader(data);
        reader.ReadByte();
        var packet = new MatchPlantIgnitePacket();
        packet.Deserialize(reader);
        Callable.From(() => OnPlantIgnited?.Invoke(packet)).CallDeferred();
    }

    private void HandleIrrigateVFX(CSteamID sender, byte[] data)
    {
        var reader = new PacketReader(data);
        reader.ReadByte();
        var packet = new MatchIrrigateVFXPacket();
        packet.Deserialize(reader);
        Callable.From(() => OnIrrigateVFX?.Invoke(packet)).CallDeferred();
    }

    private void HandleProjectileSpawn(CSteamID sender, byte[] data)
    {
        var reader = new PacketReader(data);
        reader.ReadByte();
        var packet = new MatchProjectileSpawnPacket();
        packet.Deserialize(reader);
        Callable.From(() => OnProjectileSpawned?.Invoke(packet)).CallDeferred();
    }
}