using Godot;

public partial class PlayerInput : Node
{
    public int PlayerId { get; private set; }
    public int DeviceId { get; private set; }
    public string DeviceType { get; private set; }

    private Vector2 moveInput;
    public Vector2 MoveInput => moveInput;
    public Vector2 FacingDir { get; private set; } = Vector2.Down;

    private bool canControl = true;
    public bool CanControl => canControl;

    public bool InteractPressed { get; private set; }
    public bool PickupPressed { get; private set; }
    public bool DropPressed { get; private set; }
    public bool SabotagePressed { get; private set; }
    public int? SlotPressed { get; private set; }

    public override void _Ready()
    {
        InputDeviceManager.Instance.InputReceived += OnInputReceived;
    }

    public void AssignDevice(int deviceId, string type, int playerId)
    {
        DeviceId = deviceId;
        DeviceType = type;
        PlayerId = playerId;

        GD.Print($"Player {PlayerId} assigned → {DeviceType} ({DeviceId})");
    }

    public void ToggleControl()
	{
		canControl = !canControl;
	}

    private void OnInputReceived(int playerId, InputEvent @event)
    {
        if (playerId != PlayerId || !canControl)
            return;

        if (DeviceType == "Keyboard" && @event is InputEventKey key && key.Pressed)
        {
            if (key.Keycode == Key.E)
                InteractPressed = true;

            if (key.Keycode == Key.Q)
                PickupPressed = true;

            if (key.Keycode == Key.G)
                DropPressed = true;

            if (key.Keycode == Key.F)
                SabotagePressed = true;
            
            if (key.Keycode == Key.Key1) SlotPressed = 0;
            if (key.Keycode == Key.Key2) SlotPressed = 1;
            if (key.Keycode == Key.Key3) SlotPressed = 2;
            if (key.Keycode == Key.Key4) SlotPressed = 3;
        }

        if (DeviceType == "Controller" && @event is InputEventJoypadButton btn && btn.Pressed)
        {
            if (btn.ButtonIndex == JoyButton.A)
                InteractPressed = true;

            if (btn.ButtonIndex == JoyButton.X)
                PickupPressed = true;

            if (btn.ButtonIndex == JoyButton.B)
                DropPressed = true;

            if (btn.ButtonIndex == JoyButton.Y)
                SabotagePressed = true;

            if (btn.ButtonIndex == JoyButton.DpadLeft) SlotPressed = 0;
            if (btn.ButtonIndex == JoyButton.DpadUp) SlotPressed = 1;
            if (btn.ButtonIndex == JoyButton.DpadRight) SlotPressed = 2;
            if (btn.ButtonIndex == JoyButton.DpadDown) SlotPressed = 3;
        }
    }

    public override void _Process(double delta)
    {
        if (DeviceType == "Keyboard")
        {
            float x = 0;
            float y = 0;

            if (Input.IsKeyPressed(Key.A)) x -= 1;
            if (Input.IsKeyPressed(Key.D)) x += 1;
            if (Input.IsKeyPressed(Key.W)) y -= 1;
            if (Input.IsKeyPressed(Key.S)) y += 1;

            moveInput = new Vector2(x, y);
        }
        else if (DeviceType == "Controller")
        {
            float x = Input.GetJoyAxis(DeviceId, JoyAxis.LeftX);
            float y = Input.GetJoyAxis(DeviceId, JoyAxis.LeftY);

            moveInput = new Vector2(x, y);

            if (moveInput.Length() < 0.2f)
                moveInput = Vector2.Zero;
        }

        if (!canControl)
            moveInput = Vector2.Zero;
        
        moveInput = moveInput.Normalized();

        UpdateFacingDir();
        ClearFrameInput();
    }

    private void UpdateFacingDir()
	{
		if (moveInput == Vector2.Zero)
			return;

		FacingDir = moveInput;
	}

    private void ClearFrameInput()
    {
        InteractPressed = false;
        PickupPressed = false;
        DropPressed = false;
        SabotagePressed = false;
    }
}