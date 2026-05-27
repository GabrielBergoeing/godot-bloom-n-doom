using Godot;
using System;
using Steamworks;
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

    private readonly Dictionary<
        ulong,
        LobbyPlayerStatePacket
    > _players = new();

    public IReadOnlyDictionary<
        ulong,
        LobbyPlayerStatePacket
    > Players => _players;

    private Callback<LobbyCreated_t> _lobbyCreated;
    private Callback<LobbyEnter_t> _lobbyEntered;
    private Callback<GameLobbyJoinRequested_t> _joinRequest;
    private Callback<LobbyChatUpdate_t> _lobbyChatUpdate;

    public override void _Ready()
    {
        Instance = this;
        RegisterCallbacks();
        GD.Print("[SteamLobbyManager] Ready");
    }

    public void Initialize(SteamPacketRouter router)
    {
        router.RegisterHandler(
            (byte)NetworkPacketType.LobbyPlayerState,
            HandleLobbyPlayerState
        );
    }

    public void HandleLobbyPlayerState(CSteamID sender, byte[] data)
    {
        LobbyPlayerStatePacket packet = LobbyPlayerStatePacket
            .FromBytes(data);

        _players[sender.m_SteamID] = packet;
        OnPlayerStateUpdated?.Invoke(packet);

        GD.Print($"Updated player state for {sender}");
    }

    private void RegisterCallbacks()
    {
        _lobbyCreated = Callback<LobbyCreated_t>.Create(
            OnLobbyCreated
        );

        _lobbyEntered = Callback<LobbyEnter_t>.Create(
            OnLobbyEntered
        );

        _joinRequest = Callback<GameLobbyJoinRequested_t>.Create(
            OnJoinRequested
        );

        _lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(
            OnLobbyChatUpdate
        );
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

        _players.Clear();
        Network.Connection.Clear();

        GD.Print("[SteamLobbyManager] Left lobby");
    }

    public void InviteFriend(CSteamID friendId)
    {
        if (!CurrentLobbyId.IsValid())
            return;

        SteamMatchmaking.InviteUserToLobby(
            CurrentLobbyId,
            friendId
        );
    }

    public void Broadcast(NetworkPacket packet)
    {
        GD.Print($"Broadcasting packet to {Network.Connection.GetAllPeers()} peers");

        foreach (var peer in Network.Connection.GetAllPeers())
        {
            GD.Print($"Sending packet to {peer}");
            Network.Steam.SendPacket(peer, packet);
        }
    }

    public void UpdatePlayerState(LobbyPlayerData player, int characterIndex)
    {
        if (!Network.IsOnline)
            return;

        LobbyPlayerStatePacket packet = new LobbyPlayerStatePacket
        {
            SteamId = SteamUser.GetSteamID().m_SteamID,
            Username = SteamFriends.GetPersonaName(),
            PlayerId = player.PlayerId,
            CharacterIndex = characterIndex,
            LockedIn = player.LockedIn
        };

        Broadcast(packet);
        GD.Print($"Peers: {Network.Connection.GetAllPeers()}");
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
        HostSteamId = SteamUser.GetSteamID().m_SteamID;

        GD.Print($"Lobby created: {CurrentLobbyId}");
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        GD.Print("[SteamLobbyManager] OnLobbyEntered fired");
        CurrentLobbyId = new CSteamID(callback.m_ulSteamIDLobby);

        HostSteamId = SteamMatchmaking
            .GetLobbyOwner(CurrentLobbyId)
            .m_SteamID;
        
        GD.Print($"Entered lobby: {CurrentLobbyId}");
        RegisterLobbyMembers();
        NotifyLobbyReady();
    }

    private void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
    {
        GD.Print("[SteamLobbyManager] Lobby member update");
        RegisterLobbyMembers();
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
        int count = SteamMatchmaking.GetNumLobbyMembers(
            CurrentLobbyId
        );

        Network.Connection.Clear();
        for (int i = 0; i < count; i++)
        {
            CSteamID member =SteamMatchmaking.GetLobbyMemberByIndex(
                CurrentLobbyId, i
            );

            if (member == SteamUser.GetSteamID())
                continue;

            Network.Connection.AddPeer(member);
            GD.Print($"Registered peer: {member}");
        }
    }
}