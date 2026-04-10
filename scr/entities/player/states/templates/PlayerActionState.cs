using Godot;
using System;
using System.Threading.Tasks;

public partial class PlayerActionState : PlayerState
{
    protected bool isPerformingAction = true;

    public PlayerActionState(Player player, StateMachine sm)
        : base(player, sm)
    {
    }

    public override void Enter()
    {
        base.Enter();
        isPerformingAction = true;
    }

    public override void Update(float delta)
    {
        base.Update(delta);

        player.Velocity = Vector2.Zero;

        if (!isPerformingAction)
            stateMachine.ChangeState(player.IdleState);
    }

    /*
    protected async Task ExecuteAction(
        float duration,
        float cooldown,
        Action<Vector2I> applyAction
    )
    {
        player.ToggleControl(false);

        applyAction?.Invoke(player.CurrentCell);

        await ToSignal(player.GetTree().CreateTimer(duration), "timeout");

        player.ToggleControl(true);
        isPerformingAction = false;

        if (cooldown > 0)
            await ToSignal(player.GetTree().CreateTimer(cooldown), "timeout");
    }
    */
}