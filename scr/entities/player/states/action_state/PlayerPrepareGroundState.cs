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
            stateMachine.ChangeState(player.idleState);
            return;
        }

        player.StartCoroutine(
            ExecuteAction(player.prepareGroundFrame, player.prepareGroundCooldown, cell => //Define cooldowns in player?
            {
                if (!FarmManager.instance.IsPrepared(cell) || !FarmManager.instance.IsOccupied(cell))
                {
                    FarmManager.instance.PrepareTile(cell); //Change to callable fuction
                    sfx.PlayOnPrepareGround();
                }
            })
        );
    }
}
