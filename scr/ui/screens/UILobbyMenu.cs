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
        UI.Network.Lobby.OnPlayerStateUpdated -= OnRemotePlayerUpdated;
        UI.Network.Lobby.OnPlayerStateUpdated += OnRemotePlayerUpdated;
        UI.Network.Lobby.OnPlayerLeft -= OnRemotePlayerLeft;
        UI.Network.Lobby.OnPlayerLeft += OnRemotePlayerLeft;
    }

    public override void _ExitTree()
    {
        UI.Network.Lobby.OnPlayerStateUpdated -= OnRemotePlayerUpdated;
        UI.Network.Lobby.OnPlayerLeft -= OnRemotePlayerLeft;
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
        if (HasOldSlot(player)) return;

        var slot = FindFreeSlot();
        if (slot == null) return;

        int slotIndex = System.Array.IndexOf(_slots, slot);
        slot.SlotIndex = slotIndex;
        slot.AssignPlayer(player, this);
        LobbyStateService.Instance.RegisterLocalPlayer(player, slot.Index, false);

        if (UI.Network.IsOnline)
            UI.Network.Lobby.UpdatePlayerState(player, slot.Index, slotIndex);
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
        LobbyStateService.Instance.UpdateRemoteState(packet);

        if (_remoteSlots.TryGetValue(packet.SteamId, out UICharacterSlot existingSlot))
        {
            existingSlot.AssignRemotePlayer(packet, this);
            return;
        }

        UICharacterSlot targetSlot = null;
        if (packet.SlotIndex >= 0 && packet.SlotIndex < _slots.Length && !_slots[packet.SlotIndex].Occupied)
        {
            targetSlot = _slots[packet.SlotIndex];
            GD.Print($"[UILobbyMenu] Assigning remote player to slot {packet.SlotIndex}");
        }
        else
        {
            targetSlot = FindFreeSlot();
            GD.Print($"[UILobbyMenu] SlotIndex {packet.SlotIndex} unavailable, using free slot");
        }

        if (targetSlot == null)
        {
            GD.PrintErr("[UILobbyMenu] No free slot for remote player");
            return;
        }

        targetSlot.AssignRemotePlayer(packet, this);
        _remoteSlots[packet.SteamId] = targetSlot;
        GD.Print($"[UILobbyMenu] Created remote slot for {packet.SteamId}");
    }

    private void OnRemotePlayerLeft(ulong steamId)
    {
        GD.Print($"[UILobbyMenu] Remote player left: {steamId}");

        LobbyStateService.Instance.RemoveRemotePlayer(steamId);

        if (!_remoteSlots.TryGetValue(steamId, out UICharacterSlot slot))
            return;

        slot.SetEmpty();
        _remoteSlots.Remove(steamId);
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

            UI.Network.Lobby.UpdatePlayerState(slot.Player, slot.Index, slot.SlotIndex);
        }

        GD.Print("[UILobbyMenu] Sent initial states");
    }

    private bool HasOldSlot(LobbyPlayerData player)
    {
        foreach (var slot in _slots)
        {
            if (slot.Player == player)
                return true;
        }
        return false;
    }
}