using Godot;
using System.Linq;

public partial class UILobbyMenu : Control
{
    private UIService UI => UIService.Instance;
    
    [Export] public int MinimumPlayers = 1;
    [Export] public CharacterDatabase CharacterDatabase;

    private HBoxContainer _slotsContainer;
    private UICharacterSlot[] _slots;

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

        StartMatch();
    }

    private void StartMatch()
    {
        GD.Print("START MATCH");

        // Future:
        // SceneManager
        // spawn configs
        // map selection
    }
}