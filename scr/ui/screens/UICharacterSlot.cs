using Godot;
using System;

public partial class UICharacterSlot : TextureRect
{
	private UIService UI => UIService.Instance;

	private TextureRect _illustration;
	private Label _name;
	//private TextureRect _lockIcon;

	private LobbyPlayerData _player;
	private UILobbyMenu _menu;

	private int _index = 0;
	private float _cooldown = 0f;

	[Export] public float NavCooldown = 0.2f;

	public bool Occupied => _player != null;
	public LobbyPlayerData Player => _player;

	public override void _Ready()
	{
		_illustration = GetNode<TextureRect>("Illustration");
		_name = GetNode<Label>("Name");
		//_lockIcon = GetNode<TextureRect>("LockIcon");

		SetEmpty();
	}

	public void AssignPlayer(LobbyPlayerData player, UILobbyMenu menu)
	{
		_player = player;
		_menu = menu;
		_index = 0;

		UpdateVisuals();
	}

	public override void _Process(double delta)
	{
		if (_player == null)
			return;

		_cooldown -= (float)delta;

		HandleNavigation();
		HandleConfirm();
		HandleCancel();
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
	}

	private void HandleConfirm()
	{
		if (_player.LockedIn)
			return;

		if (PressedAccept())
		{
			_player.LockedIn = true;

			_player.SelectedCharacter =
				_menu.Characters[_index];

			UpdateVisuals();

			_menu.NotifySlotUpdated();
		}
	}

	private void HandleCancel()
	{
		if (!PressedCancel())
			return;

		if (_player.LockedIn)
		{
			_player.LockedIn = false;
			UpdateVisuals();
			_menu.NotifySlotUpdated();
		}
		else
			ClearSlot();
	}

	private float GetVerticalAxis()
	{
		if (_player.DeviceType == "Keyboard")
		{
			if (Input.IsKeyPressed(Key.Up))
				return 1;

			if (Input.IsKeyPressed(Key.Down))
				return -1;
		}
		else
		{
			return Input.GetJoyAxis(
				_player.DeviceId,
				JoyAxis.LeftY
			) * -1f;
		}
		return 0;
	}

	private bool PressedAccept()
	{
		if (_player.DeviceType == "Keyboard")
			return Input.IsKeyPressed(Key.Enter);

		return Input.IsJoyButtonPressed(
			_player.DeviceId,
			JoyButton.A
		);
	}

	private bool PressedCancel()
	{
		if (_player.DeviceType == "Keyboard")
			return Input.IsKeyPressed(Key.Backspace);

		return Input.IsJoyButtonPressed(
			_player.DeviceId,
			JoyButton.B
		);
	}

	private void UpdateVisuals()
	{
		if (_player == null)
			return;

		CharacterData character = _menu.Characters[_index];

		_illustration.Texture = character.Illustration;
		_name.Text = character.CharacterName;
		//_lockIcon.Visible =_player.LockedIn;

		// Colors
		Color targetColor =
			_player.LockedIn
			? character.LockedColor
			: character.ActiveColor;

		SelfModulate = targetColor;

		GD.Print(
			$"Player {_player.PlayerId} selected {character.CharacterName}"
		);
	}

	public void SetEmpty()
	{
		SelfModulate = Colors.DarkSlateGray;
		_illustration.Texture = null;
		_name.Text = "Press 'Start' to Join";
		//_lockIcon.Visible = false;
	}

	private void ClearSlot()
	{
		InputDeviceManager.Instance
			.RemovePlayer(_player);

		_player = null;
		_menu.NotifySlotUpdated();

		SetEmpty();
	}
}
