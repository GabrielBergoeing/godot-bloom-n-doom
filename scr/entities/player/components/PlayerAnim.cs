using Godot;

public partial class PlayerAnim : AnimatedSprite2D
{
    private Player player;

    private string currentAction = "idle";

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
        currentAction = action;
    }

    private void UpdateAnimation()
    {
        string direction = GetDirectionString();

        string finalAnim = $"{currentAction}_{direction}";

        if (Animation != finalAnim)
            Play(finalAnim);

        HandleFlip(direction);
    }

    private string GetDirectionString()
    {
        Vector2 dir = player.FacingDir;

        if (Mathf.Abs(dir.X) > Mathf.Abs(dir.Y))
            return "side";

        return dir.Y > 0 ? "down" : "up";
    }

    private void HandleFlip(string direction)
    {
        if (direction == "side")
            FlipH = player.FacingDir.X < 0;
        else
            FlipH = false;
    }
}