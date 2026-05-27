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

    private readonly Dictionary<
        ulong,
        UICharacterSlot
    > _remoteSlots = new();

    public override void _Ready()
    {
        _slotsContainer = GetNode<HBoxContainer>("Slots");

        _slots = _slotsContainer
            .GetChildren()
            .OfType<Control>()
            .Select(wrapper => wrapper.GetChild<UICharacterSlot>(0))
            .ToArray();

        InputDeviceManager.Instance.PlayerJoined += OnPlayerJoined;
        foreach (var slot in _slots)
            slot.SetEmpty();
        

        if (UI.Network.IsOnline)
            SyncOnlineLobby();
    }

    private void OnPlayerJoined(LobbyPlayerData player)
    {
        var slot = FindFreeSlot();
        if (slot == null)
        {
            GD.Print("No free slot.");
            return;
        }

        slot.AssignPlayer(player, this);

        if (UI.Network.IsOnline)
            UI.Network.Lobby.UpdatePlayerState(player, 0);

        EvaluateStart();
    }

    private UICharacterSlot FindFreeSlot()
    {
        foreach (var slot in _slots)
        {
            if (!slot.Occupied)
                return slot;
        }
        return null;
    }

    public void NotifySlotUpdated()
    {
        EvaluateStart();
    }

    public CharacterData[] Characters => CharacterDatabase.Characters;

    private void EvaluateStart()
    {
        var activeSlots = _slots
            .Where(s => s.Occupied)
            .ToArray();

        if (activeSlots.Length < MinimumPlayers)
            return;

        bool allLocked = activeSlots.All(
            s => s.Player.LockedIn
        );

        if (!allLocked)
            return;

        ConfirmPlayers();
    }

    private void ConfirmPlayers()
    {
        var players = _slots
            .Where(s => s.Occupied)
            .Select(s => s.Player)
            .ToArray();

        UI.Game.SetLobbyPlayers(players);
        UI.SFX.PlayOnConfirm();
        UI.Scene.ChangeScene(UI.Paths.LevelSelectScene);
    }

    private void OnRemotePlayerUpdated(
        LobbyPlayerStatePacket packet
    )
    {
        // Ignore ourself
        if (packet.SteamId ==
            Steamworks.SteamUser.GetSteamID().m_SteamID)
            return;

        if (_remoteSlots.TryGetValue(
            packet.SteamId,
            out UICharacterSlot existingSlot
        ))
        {
            existingSlot.AssignRemotePlayer(
                packet,
                this
            );

            return;
        }

        UICharacterSlot freeSlot = FindFreeSlot();

        if (freeSlot == null)
        {
            GD.PrintErr(
                "No free slot for remote player"
            );
            return;
        }

        freeSlot.AssignRemotePlayer(
            packet,
            this
        );

        _remoteSlots[packet.SteamId] =
            freeSlot;

        GD.Print($"Created remote slot for {packet.SteamId}");
        EvaluateStart();
    }

    private void SyncOnlineLobby()
    {
        UI.Network.Lobby.OnPlayerStateUpdated += OnRemotePlayerUpdated;
        foreach (var kvp in UI.Network.Lobby.Players)
            OnRemotePlayerUpdated(kvp.Value);

        CallDeferred(nameof(SendInitialStates));
    }

    private void SendInitialStates()
    {
        foreach (UICharacterSlot slot in _slots)
        {
            if (!slot.Occupied)
                continue;

            UI.Network.Lobby.UpdatePlayerState(
                slot.Player,
                0
            );
        }

        GD.Print("[UILobbyMenu] Sent initial states");
    }
}