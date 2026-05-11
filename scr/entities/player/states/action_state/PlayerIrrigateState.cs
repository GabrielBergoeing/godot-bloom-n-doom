using Godot;
using System.Threading.Tasks;

public partial class PlayerIrrigateState : PlayerActionState
{
    public PlayerIrrigateState(Player player, StateMachine sm): base(player, sm)
    {}

    public override async void Enter()
    {
        base.Enter();

        await ExecuteAction(
            Water.Duration,
            Water.Cooldown,
            () =>
            {
                if (!Water.TryConsumeWater())
                    return;

                tile.IrrigateInCell();
                WaterFX.Play();
            }
        );
    }
}