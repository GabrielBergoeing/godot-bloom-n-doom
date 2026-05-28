using Godot;
using System;
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
        GD.Print("[SteamMatchManager] Broadcasting match start");

        Game.SetMatchRoster(packet.Players);
        Network.Lobby.Broadcast(packet);
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
        MatchStartPacket packet = new();
        int spawnIndex = 0;

        foreach (var player in Game.LobbyPlayers)
        {
            packet.Players.Add(new PlayerSpawnData
            {
                SteamId = player.SteamId,
                PlayerId = player.PlayerId,
                CharacterIndex = player.SelectedCharacter.CharacterID,
                SpawnIndex = spawnIndex++,
                IsLocalOwner = (LocalSteamId == player.SteamId)
            });
        }

        return packet;
    }

    private void HandleMatchStart(CSteamID sender, byte[] data)
    {
        PacketReader reader = new PacketReader(data);
        reader.ReadByte();

        MatchStartPacket packet = new();
        packet.Deserialize(reader);

        // Mark which players are local on this machine
        ulong localSteamId = Network.Lobby.LocalSteamId;
        foreach (var p in packet.Players)
            p.IsLocalOwner = p.SteamId == localSteamId;

        GD.Print($"[SteamMatchManager] Match start received, {packet.Players.Count} players");

        Callable.From(() =>
        {
            GameManager.Instance.SetMatchRoster(packet.Players);
            OnMatchStarted?.Invoke(packet);
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