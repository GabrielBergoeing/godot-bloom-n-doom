using Godot;
using System;

public class PlayerPrepareGroundState : PlayerActionState
{
    public PlayerPrepareGroundState(Player player, StateMachine sm) : base(player, sm) 
    {}

    public override void Enter()
    {
        base.Enter();

        _ = ExecuteAction(
            2f, //player.prepareGroundFrame,
            0.5f, //player.prepareGroundCooldown,
            () =>
            {
                tile.PrepareInCell();
                SFX.PlayOnPrepare();
            }
        );
    }
}
