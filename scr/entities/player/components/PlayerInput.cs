using Godot;

public partial class PlayerInput : Node
{
    public int PlayerId { get; private set; }
    public int DeviceId { get; private set; }
    public string DeviceType { get; private set; }

    private Vector2 moveInput;
    public Vector2 MoveInput => moveInput;

    public bool InteractPressed { get; private set; }
    public bool PickupPressed { get; private set; }
    public bool DropPressed { get; private set; }
    public bool SabotagePressed { get; private set; }

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

    private void OnInputReceived(int playerId, InputEvent @event)
    {
        if (playerId != PlayerId)
            return;

        if (@event.IsActionPressed("interact"))
            InteractPressed = true;

        if (@event.IsActionPressed("pickup"))
            PickupPressed = true;

        if (@event.IsActionPressed("drop"))
            DropPressed = true;

        if (@event.IsActionPressed("sabotage"))
            SabotagePressed = true;
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

        moveInput = moveInput.Normalized();
    }

    public void ClearFrameInput()
    {
        InteractPressed = false;
        PickupPressed = false;
        DropPressed = false;
        SabotagePressed = false;
    }
}