using Godot;

public partial class PlayerNeutralState : PlayerState
{
    private bool hasInteraction = false;

    public PlayerNeutralState(Player player, StateMachine sm): base(player, sm)
    {}

    public override void Enter()
    {
        base.Enter();
        hasInteraction = false;
    }

    public override void Update(float delta)
    {
        base.Update(delta);
  
        if (Input.SabotagePressed && !hasInteraction)
        {
            hasInteraction = true;

            var state = DetermineDisruptionState();
            if (state != null)
                stateMachine.ChangeState(state);
        }

        if (Input.InteractPressed && !hasInteraction)
        {
            hasInteraction = true;

            var state = DetermineInteractionState();
            if (state != null)
                stateMachine.ChangeState(state);
        }

    }

    private PlayerState DetermineInteractionState()
    {
        if(TryPickupNearby())
            return this;

        if(IsHandEmpty() && tile.CanRefillWater())
        {
            Water.Refill();
            return this;
        }

        if (IsHandEmpty() && tile.CanPrepare())
            return player.PrepareGroundState;

        if (IsHandEmpty() && tile.CanInteractPlant(player.PlayerId) && Water.CanWater())
            return player.IrrigateState;

        if (CanUseItem())
            return player.UseItemState;

        return this;
    }

    private bool CanUseItem()
    {
        var stack = Inventory?.GetCurrentStack();
        var ctx = new ItemUseContext(player, tile);

        return stack.Data.CanUse(ctx);
    }

    private PlayerState DetermineDisruptionState()
    {
        if (TryDroppingItem())
            return this;

        if(tile.CanInteractPlant(player.PlayerId))
            return player.RemoveState;
        
        return this;
    }
}