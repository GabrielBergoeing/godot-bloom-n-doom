using Godot;

public partial class PlayerOnHand : Node2D
{
    [Export] private float distanceFromPlayer = 16f;

    private PlayerHotbar Hotbar;
    private Node2D currentVisual;

    public override void _Ready()
    {
        Hotbar = GetParent<PlayerHotbar>();
        Hotbar.OnSlotChanged += UpdateVisual;
    }

    public override void _Process(double delta)
    {
        UpdatePosition();
    }

    public void UpdateVisual()
    {
        var stack = Hotbar.GetCurrentStack();

        if (stack == null || stack.Data.PickupScene == null)
        {
            ClearVisual();
            return;
        }

        if (currentVisual != null && currentVisual.Name == stack.Data.ItemName)
            return;
        ClearVisual();

        var instance = stack.Data.PickupScene.Instantiate<Node2D>();
        instance.Name = stack.Data.ItemName;

        AddChild(instance);
        instance.Position = Vector2.Zero;

        currentVisual = instance;
    }

    private void UpdatePosition()
    {
        Vector2 dir = Hotbar.Player.GetFacingDirection();
        if (dir == Vector2.Zero)
            return;

        Position = dir.Normalized() * distanceFromPlayer;
    }

    private void ClearVisual()
    {
        if (currentVisual != null)
        {
            currentVisual.QueueFree();
            currentVisual = null;
        }
    }
}