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

    private bool matchActive = true;


    public bool InteractPressed { get; private set; }
    public bool SabotagePressed { get; private set; }
    public int? SlotPressed { get; private set; }

    public bool ShootPressed { get; private set; }
    public bool ShootHeld { get; private set; }
    public bool ShootReleased { get; private set; }

    private bool previousShootHeld;
    private bool IsKeyboard => DeviceType == "Keyboard";
    private bool IsController => DeviceType == "Controller";

    public void Setup(int playerId, int deviceId, string type)
    {
        DeviceId = deviceId;
        DeviceType = type;
        PlayerId = playerId;
    }

    public void ToggleControl(bool newState)
    {
        canControl = newState;
    }

    public void SetMatchStatus(bool matchStatus)
    {
        bool matchActive = matchStatus;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!canControl || !matchActive)
            return;

        if (IsKeyboard)
        {
            if (@event is not InputEventKey key)
                return;

            if (!key.Pressed || key.Echo)
                return;

            if (key.Device != DeviceId)
                return;

            if (key.Keycode == Key.E)
                InteractPressed = true;

            if (key.Keycode == Key.Q)
                SabotagePressed = true;

            if (key.Keycode == Key.Key1) SlotPressed = 0;
            if (key.Keycode == Key.Key2) SlotPressed = 1;
            if (key.Keycode == Key.Key3) SlotPressed = 2;
            if (key.Keycode == Key.Key4) SlotPressed = 3;
        }

        if (IsController)
        {
            if (@event is not InputEventJoypadButton btn)
                return;

            if (!btn.Pressed)
                return;

            if (btn.Device != DeviceId)
                return;

            if (btn.ButtonIndex == JoyButton.A)
                InteractPressed = true;

            if (btn.ButtonIndex == JoyButton.B)
                SabotagePressed = true;

            if (btn.ButtonIndex == JoyButton.DpadLeft) SlotPressed = 0;
            if (btn.ButtonIndex == JoyButton.DpadUp) SlotPressed = 1;
            if (btn.ButtonIndex == JoyButton.DpadRight) SlotPressed = 2;
            if (btn.ButtonIndex == JoyButton.DpadDown) SlotPressed = 3;
        }
    }

    public override void _Process(double delta)
    {
        ClearFrameInput();
        HandleMovement();
        HandleShoot();
        UpdateFacingDir();
    }

    private void HandleMovement()
    {
        if (IsKeyboard)
        {
            float x = 0;
            float y = 0;

            if (Input.IsKeyPressed(Key.A)) x -= 1;
            if (Input.IsKeyPressed(Key.D)) x += 1;
            if (Input.IsKeyPressed(Key.W)) y -= 1;
            if (Input.IsKeyPressed(Key.S)) y += 1;

            moveInput = new Vector2(x, y);
        }
        else if (IsController)
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
    }

    private void HandleShoot()
    {
        previousShootHeld = ShootHeld;

        if (IsKeyboard)
        {
            ShootHeld = Input.IsKeyPressed(Key.Space);
        }
        else if (IsController)
        {
            ShootHeld = Input.IsJoyButtonPressed(
                DeviceId,
                JoyButton.RightShoulder
            );
        }

        ShootPressed = ShootHeld && !previousShootHeld;
        ShootReleased = !ShootHeld && previousShootHeld;
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
        SabotagePressed = false;
        SlotPressed = null;
    }
}