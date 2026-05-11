using Godot;

public partial class PlayerOnHand : Node2D
{
    [Export] private float distanceFromPlayer = 16f;

    private Player Player;
    private PlayerHotbar Hotbar;
    private Sprite2D ItemImage;

    public override void _Ready()
    {
        ItemImage = GetNode<Sprite2D>("Sprite2D");
    }

    public override void _Process(double delta)
    {
        UpdatePosition();
    }

    public void Initialize(Player player, PlayerHotbar hotbar)
    {
        Player = player;
        Hotbar = hotbar;

        Hotbar.OnSlotChanged += UpdateVisual;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if (Hotbar == null)
            return;

        var stack = Hotbar.GetCurrentStack();

        if (stack == null || stack.Data == null || stack.Data.Icon == null)
        {
            ItemImage.Visible = false;
            return;
        }

        ItemImage.Texture = stack.Data.Icon;
        ItemImage.Visible = true;
        ItemImage.ZIndex = 10;
    }

    private void UpdatePosition()
    {
        Vector2 dir = Player.GetFacingDirection();

        if (dir == Vector2.Zero)
            return;

        Position = dir.Normalized() * distanceFromPlayer;
    }
}