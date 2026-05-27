using Godot;
using System;
using Steamworks;
using System.Linq;
using System.Collections.Generic;

public partial class SteamLobbyManager : Node
{
    public static SteamLobbyManager Instance;

    public event Action OnLobbyReady;
    public event Action<LobbyPlayerStatePacket> OnPlayerStateUpdated;

    public NetworkRoot Network => NetworkRoot.Instance;
    public CSteamID CurrentLobbyId { get; private set; }

    public ulong HostSteamId { get; private set; }
    public bool IsHost => SteamUser.GetSteamID().m_SteamID == HostSteamId;
    public ulong LocalSteamId => SteamUser.GetSteamID().m_SteamID;

    private readonly Dictionary<ulong, LobbyPlayerStatePacket> _players = new();
    public IReadOnlyDictionary<ulong, LobbyPlayerStatePacket> Players => _players;

    // Scene readiness gate
    private bool _sceneReady = false;
    private readonly List<LobbyPlayerStatePacket> _pendingPackets = new();

    private Callback<LobbyCreated_t> _lobbyCreated;
    private Callback<LobbyEnter_t> _lobbyEntered;
    private Callback<GameLobbyJoinRequested_t> _joinRequest;
    private Callback<LobbyChatUpdate_t> _lobbyChatUpdate;

    public override void _Ready()
    {
        Instance = this;
        RegisterCallbacks();
        SceneManager.Instance.OnSceneReady += OnSceneReady;
        GD.Print("[SteamLobbyManager] Ready");
    }

    public void Initialize(SteamPacketRouter router)
    {
        router.RegisterHandler(
            (byte)NetworkPacketType.LobbyPlayerState,
            HandleLobbyPlayerState
        );
    }

    // Called by SceneManager once the new scene's first frame is done
    private void OnSceneReady()
    {
        _sceneReady = true;
        GD.Print($"[SteamLobbyManager] Scene ready, flushing {_pendingPackets.Count} pending packets");
        foreach (var packet in _pendingPackets)
            OnPlayerStateUpdated?.Invoke(packet);
        _pendingPackets.Clear();
    }

    public void HandleLobbyPlayerState(CSteamID sender, byte[] data)
    {
        LobbyPlayerStatePacket packet = LobbyPlayerStatePacket.FromBytes(data);
        _players[sender.m_SteamID] = packet;

        GD.Print($"Updated player state for {sender}");

        if (sender.m_SteamID == LocalSteamId)
            return;

        Callable.From(() =>
        {
            if (_sceneReady)
            {
                GD.Print("[SteamLobbyManager] Firing PlayerStateSignal");
                OnPlayerStateUpdated?.Invoke(packet);
            }
            else
            {
                GD.Print("[SteamLobbyManager] Scene not ready, queuing packet");
                _pendingPackets.Add(packet);
            }
        }).CallDeferred();
    }

    private void RegisterCallbacks()
    {
        _lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        _lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        _joinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequested);
        _lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
    }

    public void CreateLobby(int maxPlayers = 4)
    {
        SteamMatchmaking.CreateLobby(
            ELobbyType.k_ELobbyTypeFriendsOnly,
            maxPlayers
        );
        GD.Print("[SteamLobbyManager] Creating lobby...");
    }

    public void JoinLobby(CSteamID lobbyId)
    {
        SteamMatchmaking.JoinLobby(lobbyId);
    }

    public void LeaveLobby()
    {
        if (!CurrentLobbyId.IsValid())
            return;

        SteamMatchmaking.LeaveLobby(CurrentLobbyId);
        CurrentLobbyId = default;
        _sceneReady = false;
        _pendingPackets.Clear();
        _players.Clear();
        Network.Connection.Clear();

        GD.Print("[SteamLobbyManager] Left lobby");
    }

    public void InviteFriend(CSteamID friendId)
    {
        if (!CurrentLobbyId.IsValid())
            return;

        SteamMatchmaking.InviteUserToLobby(CurrentLobbyId, friendId);
    }

    public void Broadcast(NetworkPacket packet)
    {
        var peers = Network.Connection.GetAllPeers().ToList();
        GD.Print($"Broadcasting packet to {peers.Count} peers");

        foreach (var peer in peers)
        {
            GD.Print($"Sending packet to {peer}");
            Network.Steam.SendPacket(peer, packet);
        }
    }

    public void UpdatePlayerState(LobbyPlayerData player, int characterIndex)
    {
        if (!Network.IsOnline) return;

        LobbyPlayerStatePacket packet = new LobbyPlayerStatePacket
        {
            SteamId = LocalSteamId,
            Username = SteamFriends.GetPersonaName(),
            PlayerId = player.PlayerId,
            CharacterIndex = characterIndex,
            LockedIn = player.LockedIn
        };

        _players[packet.SteamId] = packet;
        Broadcast(packet);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        GD.Print("[SteamLobbyManager] OnLobbyCreated fired");
        GD.Print($"Result: {callback.m_eResult}");
        GD.Print($"Lobby ID: {callback.m_ulSteamIDLobby}");

        if (callback.m_eResult != EResult.k_EResultOK)
        {
            GD.PrintErr($"Failed creating lobby: {callback.m_eResult}");
            return;
        }

        CurrentLobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        HostSteamId = LocalSteamId;

        GD.Print($"Lobby created: {CurrentLobbyId}");
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        GD.Print("[SteamLobbyManager] OnLobbyEntered fired");
        CurrentLobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        HostSteamId = SteamMatchmaking.GetLobbyOwner(CurrentLobbyId).m_SteamID;
        _sceneReady = false;

        GD.Print($"Entered lobby: {CurrentLobbyId}");
        RegisterLobbyMembers();
        NotifyLobbyReady();
    }

    private void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
    {
        GD.Print("[SteamLobbyManager] Lobby member update");
        RegisterLobbyMembers();
        CallDeferred(nameof(EmitInitialPlayerState));
    }

    private void NotifyLobbyReady()
    {
        GD.Print("[SteamLobbyManager] NotifyLobbyReady");

        if (OnLobbyReady == null)
        {
            GD.PrintErr("NO SUBSCRIBERS");
            return;
        }

        GD.Print("Invoking lobby ready");
        OnLobbyReady.Invoke();
    }

    private void OnJoinRequested(GameLobbyJoinRequested_t callback)
    {
        JoinLobby(callback.m_steamIDLobby);
    }

    private void RegisterLobbyMembers()
    {
        int count = SteamMatchmaking.GetNumLobbyMembers(CurrentLobbyId);

        Network.Connection.Clear();
        for (int i = 0; i < count; i++)
        {
            CSteamID member = SteamMatchmaking.GetLobbyMemberByIndex(CurrentLobbyId, i);

            if (member == SteamUser.GetSteamID())
                continue;

            Network.Connection.AddPeer(member);
            GD.Print($"Registered peer: {member}");
        }
    }

    private void EmitInitialPlayerState()
    {
        GD.Print("[SteamLobbyManager] EmitInitialPlayerState");

        foreach (var kvp in _players)
        {
            GD.Print($"[SteamLobbyManager] Re-broadcasting state for {kvp.Key}");
            Broadcast(kvp.Value);
        }
    }
}