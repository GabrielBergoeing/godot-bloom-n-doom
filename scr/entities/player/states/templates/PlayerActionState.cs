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

    protected async Task ExecuteAction(
    float duration,
    float cooldown,
    Action<Vector2I> applyAction)
    {
        player.ToggleControl(false);

        applyAction?.Invoke(tile.CurrentCell);

        // Action duration
        await ToSignal(player.GetTree().CreateTimer(duration), SceneTreeTimer.SignalName.Timeout);

        player.ToggleControl(true);
        isPerformingAction = false;

        // Cooldown (optional, does NOT block state exit)
        if (cooldown > 0)
            await ToSignal(player.GetTree().CreateTimer(cooldown), SceneTreeTimer.SignalName.Timeout);
    }
}