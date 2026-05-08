using Godot;

public partial class PlayerShootState : PlayerActionState
{
    private ToolData tool;

    public PlayerShootState(Player player, StateMachine sm): base(player, sm)
    {}

    public override void Enter()
    {
        base.Enter();

        tool = (ToolData) Inventory.GetCurrentStack().Data;
        var ctx = new ItemUseContext(player, tile);
        tool.BeginUse(ctx);
    }

    public override void Update(float delta)
    {
        base.Update(delta);

        if (tool == null)
            return;

        var ctx = new ItemUseContext(player, tile);
        tool.TickUse(ctx, delta);

        if (!Input.ShootHeld)
        {
            tool.EndUse(ctx);
            isPerformingAction = false;
        }
    }
}