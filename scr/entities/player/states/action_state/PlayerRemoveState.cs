using Godot;
using System;

public partial class PlayerRemoveState : PlayerActionState
{
    public PlayerRemoveState(Player player, StateMachine sm) : base(player, sm) 
    {}

    public override void Enter()
    {
        base.Enter();

        _ = ExecuteAction(
            2f, //player.sabotageFrame,
            0.5f, //player.sabotageCooldown,
            () =>
            {
                tile.RemoveInCell(player.PlayerId);
                //sfx.PlayOnSabotage();
            }
        );
    }
}
