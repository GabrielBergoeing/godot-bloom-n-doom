using Godot;

public partial class PlayerAnim : AnimatedSprite2D
{
    private Player player;

    private string CurrentAction = "idle";

    public override void _Ready()
    {
        player = GetParent<Player>();
    }

    public override void _Process(double delta)
    {
        UpdateAnimation();
    }

    public void SetAction(string action)
    {
        CurrentAction = action;
    }

    private void UpdateAnimation()
    {
        string direction = GetDirectionString();

        string finalAnim = $"{CurrentAction}_{direction}";

        if (Animation != finalAnim)
            Play(finalAnim);

        HandleFlip(direction);
    }

    private string GetDirectionString()
    {
        Vector2 dir = player.GetFacingDirection();

        if (Mathf.Abs(dir.X) > Mathf.Abs(dir.Y))
            return "side";

        return dir.Y > 0 ? "down" : "up";
    }

    private void HandleFlip(string direction)
    {
        if (direction == "side")
            FlipH = player.GetFacingDirection().X < 0;
        else
            FlipH = false;
    }
}