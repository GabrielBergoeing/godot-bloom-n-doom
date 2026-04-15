using Godot;
using System;

public class PlayerPrepareGroundState : PlayerActionState
{
    public PlayerPrepareGroundState(Player player, StateMachine sm) : base(player, sm) 
    {}

    public override void Enter()
    {
        base.Enter();

        if (!IsOnHandEmpty())
        {
            stateMachine.ChangeState(player.IdleState);
            return;
        }
        _ = ExecuteAction(0,0,//player.prepareGroundFrame, player.prepareGroundCooldown,
            _ =>
            {
                if (tile.CanPrepare())
                {
                    tile.PrepareInCell();
                    //player.Sfx.PlayOnPrepareGround();
                }
            }
        );
    }
}
