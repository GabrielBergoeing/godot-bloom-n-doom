using Godot;
using System.Linq;
using System.Collections.Generic;

public partial class UILobbyMenu : Control
{
    private UIService UI => UIService.Instance;
    private SteamLobbyManager Lobby => UI.Network.Lobby;
    private LobbyStateService State => LobbyStateService.Instance;

    [Export(PropertyHint.Range, "1,4")] public int MinimumPlayers = 1;
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
        //State.Clear();
    }

    public override void _Ready()
    {
        GD.Print("[UILobbyMenu] _Ready called");

        _slotsContainer = GetNode<HBoxContainer>("Slots");
        _slots = _slotsContainer
            .GetChildren()
            .OfType<Control>()
            .Select(wrapper => wrapper.GetChild<UICharacterSlot>(0))
            .ToArray();

        GD.Print($"[UILobbyMenu] Found {_slots.Length} slots");
        foreach (var slot in _slots)
            slot.SetEmpty();

        SetupSignals();
        if (UI.Network.IsOnline)
            SyncOnlineLobby();
    }

    public override void _Process(double delta)
    {
        if (!UI.Network.IsOnline || _slots == null) return;

        foreach (var kvp in Lobby.Players)
        {
            if (kvp.Key == Lobby.LocalSteamId) continue;
            if (_remoteSlots.ContainsKey(kvp.Key)) continue;

            GD.Print($"[UILobbyMenu] Late-discovered player: {kvp.Key}");
            OnRemotePlayerUpdated(kvp.Value);
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

        GD.Print($"[UILobbyMenu] player's Steam ID is {player.SteamId}");

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
            GD.Print("[UILobbyMenu] Countdown cancelled");

        State.MinimumPlayers = MinimumPlayers;
        State.OnAllReady += ConfirmPlayers;
    }

    private void OnRemotePlayerUpdated(LobbyPlayerStatePacket packet)
    {
        GD.Print($"[UILobbyMenu] Remote update from {packet.SteamId}");

        if (_slots == null)
        {
            GD.PrintErr("[UILobbyMenu] Slots not ready, dropping packet");
            return;
        }

        if (packet.SteamId == Lobby.LocalSteamId) return;

        GD.Print("[UILobbyMenu] Processing remote packet");
        State.UpdateRemoteState(packet);

        if (_remoteSlots.TryGetValue(packet.SteamId, out var existing))
        {
            existing.AssignRemotePlayer(packet, this);
            return;
        }

        var target = ResolveTargetSlot(packet.SlotIndex);
        if (target == null)
        {
            GD.PrintErr("[UILobbyMenu] No slot available for remote player");
            return;
        }

        target.AssignRemotePlayer(packet, this);
        _remoteSlots[packet.SteamId] = target;
        GD.Print($"[UILobbyMenu] Created remote slot for {packet.SteamId}");
    }

    private void OnRemotePlayerLeft(ulong steamId)
    {
        GD.Print($"[UILobbyMenu] Remote player left: {steamId}");

        State.RemoveRemotePlayer(steamId);

        if (!_remoteSlots.TryGetValue(steamId, out var slot)) return;

        slot.SetEmpty();
        _remoteSlots.Remove(steamId);
    }

    private void ConfirmPlayers()
    {
        GD.Print("[UILobbyMenu] All ready, storing players and starting countdown");

        var players = _slots
            .Where(s => s.Occupied && !s.IsRemote)
            .Select(s => s.Player)
            .ToArray();

        UI.Game.SetLobbyPlayers(players);
        _countdown.Begin();
    }

    private void ExecuteConfirm()
    {
        GD.Print("[UILobbyMenu] Transitioning to level select");
        UI.SFX.PlayOnConfirm();
        UI.Scene.ChangeScene(UI.Paths.LevelSelectScene);
    }

    private void SyncOnlineLobby()
    {
        GD.Print("[UILobbyMenu] SyncOnlineLobby");

        foreach (var kvp in Lobby.Players)
        {
            GD.Print($"[UILobbyMenu] Existing player: {kvp.Key}");
            OnRemotePlayerUpdated(kvp.Value);
        }

        CallDeferred(nameof(SendInitialStates));
    }

    private void SendInitialStates()
    {
        foreach (var slot in _slots)
        {
            if (!slot.Occupied || slot.IsRemote) continue;
            Lobby.UpdatePlayerState(slot.Player, slot.Index, slot.SlotIndex);
        }

        GD.Print("[UILobbyMenu] Sent initial states");
    }

    private UICharacterSlot FindFreeSlot()
    {
        if (_slots == null) return null;
        return _slots.FirstOrDefault(s => !s.Occupied);
    }

    private UICharacterSlot ResolveTargetSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < _slots.Length && !_slots[slotIndex].Occupied)
        {
            GD.Print($"[UILobbyMenu] Assigning to requested slot {slotIndex}");
            return _slots[slotIndex];
        }

        GD.Print($"[UILobbyMenu] Slot {slotIndex} unavailable, finding free slot");
        return FindFreeSlot();
    }

    private bool HasSlot(LobbyPlayerData player)
    {
        return _slots.Any(s => s.Player == player);
    }
}