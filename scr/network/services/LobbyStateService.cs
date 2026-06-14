using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class LobbyStateService : Node
{
    public static LobbyStateService Instance { get; private set; }

    public event Action OnStateChanged;
    public event Action OnAllReady;

    private readonly Dictionary<(ulong steamId, int playerId), LobbyPlayerStatePacket> _remoteStates = new();
    private readonly List<LocalPlayerState> _localStates = new();

    public IReadOnlyList<LocalPlayerState> LocalStates => _localStates;
    public IReadOnlyDictionary<(ulong steamId, int playerId), LobbyPlayerStatePacket> RemoteStates => _remoteStates;

    public int MinimumPlayers { get; set; } = 1;

    public override void _Ready()
    {
        Instance = this;
    }

    public void RegisterLocalPlayer(LobbyPlayerData player, int characterIndex, bool lockedIn)
    {
        var existing = _localStates.Find(s => s.Player.PlayerId == player.PlayerId);
        if (existing != null)
        {
            existing.CharacterIndex = characterIndex;
            existing.LockedIn = lockedIn;
        }
        else
        {
            _localStates.Add(new LocalPlayerState
            {
                Player = player,
                CharacterIndex = characterIndex,
                LockedIn = lockedIn
            });
        }

        OnStateChanged?.Invoke();
        EvaluateReady();
    }

    public void RemoveLocalPlayer(LobbyPlayerData player)
    {
        _localStates.RemoveAll(s => s.Player.PlayerId == player.PlayerId);
        OnStateChanged?.Invoke();
    }

    public void UpdateRemoteState(LobbyPlayerStatePacket packet)
    {
        _remoteStates[(packet.SteamId, packet.PlayerId)] = packet;
        OnStateChanged?.Invoke();
        EvaluateReady();
    }

    public void RemoveRemotePlayer(ulong steamId)
    {
        var keys = _remoteStates.Keys.Where(k => k.steamId == steamId).ToList();
        foreach (var key in keys)
            _remoteStates.Remove(key);
        OnStateChanged?.Invoke();
    }

    public void Clear()
    {
        _remoteStates.Clear();
        _localStates.Clear();
    }

    public int GetPlayerChar(int playerId)
    {
        var state = _localStates.Find(s => s.Player.PlayerId == playerId);
        if (state != null)
            return state.CharacterIndex;
        return -1;
    }

    private void EvaluateReady()
    {
        int total = _localStates.Count + _remoteStates.Count;
        if (total < MinimumPlayers) return;
        if (_localStates.Count == 0) return;

        bool localReady = _localStates.TrueForAll(s => s.LockedIn);
        bool remoteReady = true;
        foreach (var kvp in _remoteStates)
            if (!kvp.Value.LockedIn) remoteReady = false;

        if (localReady && remoteReady)
            OnAllReady?.Invoke();
    }

    public void ClearLocalStates()
    {
        _localStates.Clear();
        OnStateChanged?.Invoke();
    }

    public void ResetLockedStates()
    {
        foreach (var state in _localStates)
            state.LockedIn = false;

        var keys = new List<(ulong, int)>(_remoteStates.Keys);
        foreach (var key in keys)
        {
            var packet = _remoteStates[key];
            packet.LockedIn = false;
            _remoteStates[key] = packet;
        }

        OnStateChanged?.Invoke();
    }
}

public class LocalPlayerState
{
    public LobbyPlayerData Player;
    public int CharacterIndex;
    public bool LockedIn;
}