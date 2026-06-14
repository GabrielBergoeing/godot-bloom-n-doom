using Godot;
using System.Linq;
using System.Collections.Generic;

public partial class UILobbyMenu : Control
{
    private UIService UI => UIService.Instance;
    private SteamLobbyManager Lobby => UI.Network.Lobby;
    private LobbyStateService State => LobbyStateService.Instance;

    [Export(PropertyHint.Range, "1,4")] public int MinimumPlayers = 2;
    [Export] public CharacterDatabase CharacterDatabase;
    [Export] private UILobbyCountdown _countdown;

    private HBoxContainer _slotsContainer;
    private UICharacterSlot[] _slots;

    private readonly Dictionary<ulong, UICharacterSlot> _remoteSlots = new();

    public CharacterData[] Characters => CharacterDatabase.Characters;

    public override void _EnterTree()
    {
        Lobby.OnPlayerStateUpdated -= OnRemotePlayerUpdated;
        Lobby.OnPlayerStateUpdated += OnRemotePlayerUpdated;
        Lobby.OnPlayerLeft -= OnRemotePlayerLeft;
        Lobby.OnPlayerLeft += OnRemotePlayerLeft;
    }

    public override void _ExitTree()
    {
        Lobby.OnPlayerStateUpdated -= OnRemotePlayerUpdated;
        Lobby.OnPlayerLeft -= OnRemotePlayerLeft;
        _countdown.OnCountdownComplete -= ExecuteConfirm;
        State.OnAllReady -= ConfirmPlayers;

        State.ClearLocalStates();
    }

    public override void _Ready()
    {
        _slotsContainer = GetNode<HBoxContainer>("Slots");
        _slots = _slotsContainer
            .GetChildren()
            .OfType<Control>()
            .Select(wrapper => wrapper.GetChild<UICharacterSlot>(0))
            .ToArray();

        foreach (var slot in _slots)
            slot.SetEmpty();

        SetupSignals();
        if (UI.Network.IsOnline)
        {
            State.ResetLockedStates();
            SyncOnlineLobby();
        }
    }

    public override void _Process(double delta)
    {
        if (!UI.Network.IsOnline || _slots == null) return;

        foreach (var packet in Lobby.Players)
        {
            if (packet.SteamId == Lobby.LocalSteamId) continue;

            ulong key = (ulong)((long)packet.SteamId ^ ((long)packet.PlayerId << 32));
            if (_remoteSlots.ContainsKey(key)) continue;

            OnRemotePlayerUpdated(packet);
        }
    }

    private void OnPlayerJoined(LobbyPlayerData player)
    {
        if (HasSlot(player)) return;

        var slot = FindFreeSlot();
        if (slot == null) return;

        player.SteamId = UI.Network.IsOnline
            ? UI.Network.Lobby.LocalSteamId
            : 0;

        int slotIndex = System.Array.IndexOf(_slots, slot);
        slot.SlotIndex = slotIndex;
        slot.AssignPlayer(player, this);

        State.RegisterLocalPlayer(player, slot.Index, false);

        if (UI.Network.IsOnline)
            Lobby.UpdatePlayerState(player, slot.Index, slotIndex);
    }

    public void NotifySlotUpdated(UICharacterSlot slot)
    {
        if (slot.Player == null) return;

        if (_countdown.IsRunning && !slot.Player.LockedIn)
            _countdown.Cancel();

        State.RegisterLocalPlayer(slot.Player, slot.Index, slot.Player.LockedIn);
    }

    private void SetupSignals()
    {
        InputDeviceManager.Instance.PlayerJoined += OnPlayerJoined;

        _countdown.OnCountdownComplete += ExecuteConfirm;
        _countdown.OnCountdownCancelled += () =>

        State.MinimumPlayers = MinimumPlayers;
        State.OnAllReady += ConfirmPlayers;
    }

    private void OnRemotePlayerUpdated(LobbyPlayerStatePacket packet)
    {
        if (_slots == null) { GD.PrintErr("[UILobbyMenu] Slots not ready, dropping packet"); return; }
        if (packet.SteamId == Lobby.LocalSteamId) return;
        State.UpdateRemoteState(packet);

        ulong key = RemoteKey(packet.SteamId, packet.PlayerId);
        if (_remoteSlots.TryGetValue(key, out var existing))
        {
            existing.AssignRemotePlayer(packet, this);
            return;
        }

        var target = ResolveTargetSlot(packet.SlotIndex);
        if (target == null) { GD.PrintErr("[UILobbyMenu] No slot available for remote player"); return; }

        target.AssignRemotePlayer(packet, this);
        _remoteSlots[key] = target;
    }

    private void OnRemotePlayerLeft(ulong steamId)
    {
        State.RemoveRemotePlayer(steamId);

        var keysToRemove = _remoteSlots.Keys
            .Where(k => (k & 0xFFFFFFFF00000000UL) == (steamId & 0xFFFFFFFF00000000UL))
            .ToList();
        foreach (var key in keysToRemove)
        {
            _remoteSlots[key].SetEmpty();
            _remoteSlots.Remove(key);
        }
    }

    private void ConfirmPlayers()
    {
        var players = _slots
            .Where(s => s.Occupied && !s.IsRemote)
            .Select(s => s.Player)
            .ToArray();

        UI.Game.SetLobbyPlayers(players);
        _countdown.Begin();
    }

    private void ExecuteConfirm()
    {
        UI.SFX.PlayOnConfirm();
        UI.Scene.ChangeScene(UI.Paths.LevelSelectScene);
    }

    private void SyncOnlineLobby()
    {
        foreach (var kvp in Lobby.Players)
            OnRemotePlayerUpdated(kvp);

        CallDeferred(nameof(SendInitialStates));
    }

    private void SendInitialStates()
    {
        foreach (var slot in _slots)
        {
            if (!slot.Occupied || slot.IsRemote) continue;
            Lobby.UpdatePlayerState(slot.Player, slot.Index, slot.SlotIndex);
        }

        if (UI.Network.IsOnline)
            Lobby.RequestStateResync();
    }

    private UICharacterSlot FindFreeSlot()
    {
        if (_slots == null) return null;
        return _slots.FirstOrDefault(s => !s.Occupied);
    }

    private UICharacterSlot ResolveTargetSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < _slots.Length && !_slots[slotIndex].Occupied)
            return _slots[slotIndex];

        return FindFreeSlot();
    }

    private bool HasSlot(LobbyPlayerData player)
    {
        return _slots.Any(s => s.Player == player);
    }

    private static ulong RemoteKey(ulong steamId, int playerId) =>
        (steamId & 0xFFFFFFFF00000000UL) | ((ulong)(uint)playerId);
}