using Godot;
using System;
using System.Threading.Tasks;

public partial class PlayerActionState : PlayerState
{
    protected bool isPerformingAction = true;

    public PlayerActionState(Player player, StateMachine sm): base(player, sm)
    {}

    public override void Enter()
    {
        base.Enter();
        isPerformingAction = true;
    }

    public override void Update(float delta)
    {
        base.Update(delta);

        player.SetMoveVelocity(Vector2.Zero);

        if (!isPerformingAction)
            stateMachine.ChangeState(player.IdleState);
    }

    protected async Task ExecuteAction(
        float duration,
        float cooldown,
        Action applyAction)
    {
        Input.ToggleControl(false);

        applyAction?.Invoke();

        await player.ToSignal(
            player.GetTree().CreateTimer(duration),
            SceneTreeTimer.SignalName.Timeout
        );

        Input.ToggleControl(true);
        isPerformingAction = false;

        if (cooldown > 0)
        {
            await player.ToSignal(
                player.GetTree().CreateTimer(cooldown),
                SceneTreeTimer.SignalName.Timeout
            );
        }
    }
}