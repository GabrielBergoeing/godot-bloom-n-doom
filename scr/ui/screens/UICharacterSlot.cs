using Godot;
using System;
using System.Collections.Generic;

public partial class UICharacterSlot : TextureRect
{
    private UIService UI => UIService.Instance;

    private TextureRect _illustration;
    private Label _name;
    private TextureRect _lockIcon;

    private LobbyPlayerData _player;
    private UILobbyMenu _menu;

	public int SlotIndex { get; set; }

    private int _index = 0;
    public int Index => _index;
    public bool IsRemote => _isRemote;

    private float _cooldown = 0f;
    private float _inputBlockTimer = 0f;

    private bool _isRemote;
    private ulong _remoteSteamId;

    [Export] public float NavCooldown = 0.2f;

    public bool Occupied => _player != null || _isRemote;
    public LobbyPlayerData Player => _player;

    public override void _Ready()
    {
        _illustration = GetNode<TextureRect>("Illustration");
        _name = GetNode<Label>("Name");
        _lockIcon = GetNode<TextureRect>("LockIcon");

        SetEmpty();
    }

    public void AssignPlayer(LobbyPlayerData player, UILobbyMenu menu)
    {
        _player = player;
        _menu = menu;
        _isRemote = false;

        _inputBlockTimer = 0.25f;
        _index = 0;

        UpdateVisuals();
    }

    public override void _Process(double delta)
    {
        if (_player == null && !_isRemote)
            return;

        if (_isRemote)
            return;

        _cooldown -= (float)delta;

        if (_inputBlockTimer > 0f)
        {
            _inputBlockTimer -= (float)delta;
            return;
        }

        HandleNavigation();
        HandleConfirm();
        HandleCancel();
    }

    public void SetEmpty()
    {
        _isRemote = false;
        _remoteSteamId = 0;
        _player = null;

        SelfModulate = Colors.DarkSlateGray;
        _illustration.Texture = null;
        _name.Text = "Press 'Start' to Join";
        _lockIcon.Visible = false;
    }

    public void AssignRemotePlayer(LobbyPlayerStatePacket packet, UILobbyMenu menu)
    {
        _menu = menu;
        _isRemote = true;
        _player = null;
        _remoteSteamId = packet.SteamId;
        _index = packet.CharacterIndex;

        if (_menu.Characters == null || _menu.Characters.Length == 0)
        {
            GD.PrintErr("[UICharacterSlot] CharacterDatabase not set or empty");
            return;
        }

        _index = Mathf.Clamp(_index, 0, _menu.Characters.Length - 1);
        CharacterData character = _menu.Characters[_index];

        _illustration.Texture = character.Illustration;
        _name.Text = packet.Username;
        _lockIcon.Visible = packet.LockedIn;
        SelfModulate = packet.LockedIn ? character.LockedColor : character.ActiveColor;
    }

    private void HandleNavigation()
    {
        if (_player.LockedIn)
            return;

        if (_cooldown > 0)
            return;

        float axis = GetVerticalAxis();
        if (Mathf.Abs(axis) < 0.5f)
            return;

        int count = _menu.Characters.Length;
        _index = (_index + (axis > 0 ? 1 : -1) + count) % count;
        _cooldown = NavCooldown;

        UpdateVisuals();
        UpdateNetwork();
    }

    private void HandleConfirm()
    {
        if (_player.LockedIn)
            return;

        if (PressedAccept())
        {
            _player.LockedIn = true;
            _player.SelectedCharacter = _menu.Characters[_index];

            UpdateVisuals();
            UpdateNetwork();
            _menu.NotifySlotUpdated(this);
        }
    }

    private void HandleCancel()
    {
        if (!PressedCancel()) return;

        if (_player.LockedIn)
        {
            _player.LockedIn = false;
            UpdateVisuals();
            UpdateNetwork();
            _menu.NotifySlotUpdated(this);
        }
        else
            ClearSlot();
    }

    private float GetVerticalAxis()
    {
        if (_player.DeviceType == "Keyboard")
        {
            if (Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.Up))
                return 1;
            if (Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.Down))
                return -1;
        }
        else
        {
            return Input.GetJoyAxis(_player.DeviceId, JoyAxis.LeftY) * -1f;
        }
        return 0;
    }

    private bool PressedAccept()
    {
        if (_player.DeviceType == "Keyboard")
            return Input.IsKeyPressed(Key.Enter);

        return Input.IsJoyButtonPressed(_player.DeviceId, JoyButton.A);
    }

    private bool PressedCancel()
    {
        if (_player.DeviceType == "Keyboard")
            return Input.IsKeyPressed(Key.Backspace);

        return Input.IsJoyButtonPressed(_player.DeviceId, JoyButton.B);
    }

    private void UpdateVisuals()
    {
        if (_player == null)
            return;

        CharacterData character = _menu.Characters[_index];

        _illustration.Texture = character.Illustration;
        _name.Text = character.CharacterName;
        _lockIcon.Visible = _player.LockedIn;

        SelfModulate = _player.LockedIn ? character.LockedColor : character.ActiveColor;

        if (_player.LockedIn)
            UI.SFX.PlayOnConfirm();
        else
            UI.SFX.PlayOnHover();
    }

	private void UpdateNetwork()
	{
		if (UI.Network.IsOnline)
			UI.Network.Lobby.UpdatePlayerState(_player, _index, SlotIndex);
	}

    private void ClearSlot()
    {
        InputDeviceManager.Instance.RemovePlayer(_player);
        LobbyStateService.Instance.RemoveLocalPlayer(_player);

        if (UI.Network.IsOnline)
            UI.Network.Lobby.BroadcastPlayerLeft();

        _player = null;
        SetEmpty();
        _menu.NotifySlotUpdated(this);
    }
}