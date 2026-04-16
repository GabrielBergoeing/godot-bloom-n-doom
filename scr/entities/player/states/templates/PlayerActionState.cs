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
        GD.Print("ExecuteAction START");

        input.ToggleControl();
        GD.Print("Control disabled");

        GD.Print($"CurrentCell: {tile.CurrentCell}");

        applyAction?.Invoke(tile.CurrentCell);
        GD.Print("Action invoked");

        GD.Print($"Waiting duration: {duration}");
        
        await player.ToSignal(
            player.GetTree().CreateTimer(duration),
            SceneTreeTimer.SignalName.Timeout
        );

        GD.Print("Duration finished");

        input.ToggleControl();
        GD.Print("Control enabled");

        isPerformingAction = false;
        GD.Print("Action completed");

        if (cooldown > 0)
        {
            GD.Print($"Waiting cooldown: {cooldown}");

            await player.ToSignal(
                player.GetTree().CreateTimer(cooldown),
                SceneTreeTimer.SignalName.Timeout
            );

            GD.Print("Cooldown finished");
        }

        GD.Print("ExecuteAction END");
    }
}