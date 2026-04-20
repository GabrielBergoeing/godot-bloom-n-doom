using Godot;
using System;

public class PlayerPrepareGroundState : PlayerActionState
{
    public PlayerPrepareGroundState(Player player, StateMachine sm) : base(player, sm) 
    {}

    public override void Enter()
    {
        base.Enter();

        _ = ExecuteAction(2f, 0.5f,//player.prepareGroundFrame, player.prepareGroundCooldown,
            _ =>
            {
                if (tile.CanPrepare())
                {
                    tile.PrepareInCell();
                    //sfx.PlayOnPrepareGround();
                }
            }
        );
    }
}
