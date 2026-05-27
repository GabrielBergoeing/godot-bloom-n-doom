using Godot;
using System.Linq;
using System.Collections.Generic;

public partial class UILobbyMenu : Control
{
    private UIService UI => UIService.Instance;

    [Export(PropertyHint.Range, "1,4")] public int MinimumPlayers = 1;
    [Export] public CharacterDatabase CharacterDatabase;

    private HBoxContainer _slotsContainer;
    private UICharacterSlot[] _slots;

    private readonly Dictionary<ulong, UICharacterSlot> _remoteSlots = new();

    public override void _EnterTree()
    {
        if (!UI.Network.IsOnline) return;
        UI.Network.Lobby.OnPlayerStateUpdated -= OnRemotePlayerUpdated;
        UI.Network.Lobby.OnPlayerStateUpdated += OnRemotePlayerUpdated;
    }

    public override void _ExitTree()
    {
        UI.Network.Lobby.OnPlayerStateUpdated -= OnRemotePlayerUpdated;
        LobbyStateService.Instance.OnAllReady -= ConfirmPlayers;
        LobbyStateService.Instance.Clear();
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

        InputDeviceManager.Instance.PlayerJoined += OnPlayerJoined;

        foreach (var slot in _slots)
            slot.SetEmpty();

        // Set up LobbyStateService
        LobbyStateService.Instance.MinimumPlayers = MinimumPlayers;
        LobbyStateService.Instance.OnAllReady -= ConfirmPlayers;
        LobbyStateService.Instance.OnAllReady += ConfirmPlayers;

        if (UI.Network.IsOnline)
            SyncOnlineLobby();
    }

    public override void _Process(double delta)
    {
        if (!UI.Network.IsOnline) return;
        if (_slots == null) return;

        foreach (var kvp in UI.Network.Lobby.Players)
        {
            if (kvp.Key == UI.Network.Lobby.LocalSteamId) continue;
            if (_remoteSlots.ContainsKey(kvp.Key)) continue;

            GD.Print($"[UILobbyMenu] Late-discovered player: {kvp.Key}");
            OnRemotePlayerUpdated(kvp.Value);
        }
    }

    private void OnPlayerJoined(LobbyPlayerData player)
    {
        var slot = FindFreeSlot();
        if (slot == null) return;

        slot.AssignPlayer(player, this);

        LobbyStateService.Instance.RegisterLocalPlayer(player, slot.Index, false);

        if (UI.Network.IsOnline)
            UI.Network.Lobby.UpdatePlayerState(player, slot.Index);
    }

    private UICharacterSlot FindFreeSlot()
    {
        if (_slots == null) return null;

        foreach (var slot in _slots)
        {
            if (!slot.Occupied)
                return slot;
        }
        return null;
    }

    public void NotifySlotUpdated(UICharacterSlot slot)
    {
        if (slot.Player == null) return;

        LobbyStateService.Instance.RegisterLocalPlayer(
            slot.Player,
            slot.Index,
            slot.Player.LockedIn
        );
    }

    public CharacterData[] Characters => CharacterDatabase.Characters;

    private void ConfirmPlayers()
    {
        var players = _slots
            .Where(s => s.Occupied && !s.IsRemote)
            .Select(s => s.Player)
            .ToArray();

        UI.Game.SetLobbyPlayers(players);
        UI.SFX.PlayOnConfirm();
        UI.Scene.ChangeScene(UI.Paths.LevelSelectScene);
    }

    private void OnRemotePlayerUpdated(LobbyPlayerStatePacket packet)
    {
        GD.Print($"[UILobbyMenu] OnRemotePlayerUpdated entered for {packet.SteamId}");

        if (_slots == null)
        {
            GD.PrintErr("[UILobbyMenu] Slots not initialized yet, dropping packet");
            return;
        }

        if (packet.SteamId == UI.Network.Lobby.LocalSteamId)
            return;

        GD.Print("[UILobbyMenu] Processing remote packet");

        // Always update LobbyStateService with remote state
        LobbyStateService.Instance.UpdateRemoteState(packet);
        if (_remoteSlots.TryGetValue(packet.SteamId, out UICharacterSlot existingSlot))
        {
            existingSlot.AssignRemotePlayer(packet, this);
            return;
        }

        UICharacterSlot freeSlot = FindFreeSlot();
        if (freeSlot == null)
        {
            GD.PrintErr("[UILobbyMenu] No free slot for remote player");
            return;
        }

        freeSlot.AssignRemotePlayer(packet, this);
        _remoteSlots[packet.SteamId] = freeSlot;

        GD.Print($"[UILobbyMenu] Created remote slot for {packet.SteamId}");
    }

    private void SyncOnlineLobby()
    {
        GD.Print("[UILobbyMenu] SyncOnlineLobby");

        foreach (var kvp in UI.Network.Lobby.Players)
        {
            GD.Print($"[UILobbyMenu] Existing player: {kvp.Key}");
            OnRemotePlayerUpdated(kvp.Value);
        }

        CallDeferred(nameof(SendInitialStates));
    }

    private void SendInitialStates()
    {
        foreach (UICharacterSlot slot in _slots)
        {
            if (!slot.Occupied) continue;

            UI.Network.Lobby.UpdatePlayerState(slot.Player, slot.Index);
        }

        GD.Print("[UILobbyMenu] Sent initial states");
    }
}