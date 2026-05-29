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
        GD.Print($"[SteamMatchManager] Broadcasting match start with {packet.Players.Count} players");

        Network.Lobby.Broadcast(packet);
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
        GD.Print("[SteamMatchManager] Building MatchStartPacket");

        MatchStartPacket packet = new();
        int spawnIndex = 0;

        foreach (var player in Game.LobbyPlayers)
        {
            int charIndex = 0;

            if (player.SelectedCharacter != null)
                charIndex = player.SelectedCharacter.CharacterID;

            ulong steamId = player.SteamId;
            bool isLocal = steamId == LocalSteamId;

            GD.Print(
                $"[SteamMatchManager] Adding player -> " +
                $"PlayerId: {player.PlayerId}, " +
                $"SteamId: {steamId}, " +
                $"Character: {charIndex}, " +
                $"Spawn: {spawnIndex}, " +
                $"Local: {isLocal}"
            );

            packet.Players.Add(
                new PlayerSpawnData
                {
                    SteamId = steamId,
                    PlayerId = player.PlayerId,
                    CharacterIndex = charIndex,
                    SpawnIndex = spawnIndex++,
                    IsLocalOwner = isLocal
                }
            );
        }

        GD.Print(
            $"[SteamMatchManager] Packet build complete " +
            $"({packet.Players.Count} players)"
        );

        return packet;
    }

    private void HandleMatchStart(CSteamID sender, byte[] data)
    {
        GD.Print($"[SteamMatchManager] HandleMatchStart from {sender.m_SteamID}");
        PacketReader reader = new PacketReader(data);
        reader.ReadByte();

        MatchStartPacket packet = new();
        packet.Deserialize(reader);

        // Mark which players are local on this machine
        ulong localSteamId = Network.Lobby.LocalSteamId;
        foreach (var p in packet.Players)
            p.IsLocalOwner = p.SteamId == localSteamId;

        GD.Print($"[SteamMatchManager] Match start received, {packet.Players.Count} players");
        Callable.From(() => OnMatchStarted?.Invoke(packet)).CallDeferred();
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