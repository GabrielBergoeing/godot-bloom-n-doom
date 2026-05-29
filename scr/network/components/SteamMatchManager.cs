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
        router.RegisterHandler(
            (byte)NetworkPacketType.MatchPlayerTransform,
            HandlePlayerTransform
        );
        router.RegisterHandler(
            (byte)NetworkPacketType.MatchStart,
            HandleMatchStart
        );
    }

    // Called by host after level is selected
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

    public void BroadcastTransform(int playerId, Vector2 position, float rotation)
    {
        MatchPlayerTransformPacket packet = new()
        {
            PlayerId = playerId,
            Position = position,
            Rotation = rotation
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

        // Remote players from LobbyStateService
        foreach (var kvp in LobbyStateService.Instance.RemoteStates)
        {
            GD.Print($"[SteamMatchManager] Adding remote -> PlayerId: {kvp.Value.PlayerId}, SteamId: {kvp.Key}, Char: {kvp.Value.CharacterIndex}, Spawn: {spawnIndex}");

            packet.Players.Add(new PlayerSpawnData
            {
                SteamId = kvp.Key,
                PlayerId = spawnIndex,
                CharacterIndex = kvp.Value.CharacterIndex,
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

        Callable.From(() =>
            OnPlayerTransformReceived?.Invoke(packet)
        ).CallDeferred();
    }
}