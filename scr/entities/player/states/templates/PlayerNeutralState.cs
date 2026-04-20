using Godot;

public partial class PlayerNeutralState : PlayerState
{
    private bool hasInteraction = false;

    public PlayerNeutralState(Player player, StateMachine sm)
        : base(player, sm)
    {}

    public override void Enter()
    {
        base.Enter();
        hasInteraction = false;
    }

    public override void Update(float delta)
    {
        base.Update(delta);
  
        /*
        if (Input.IsActionJustPressed("sabotage"))
        {
            var state = DetermineDisruptionState();
            if (state != null)
                stateMachine.ChangeState(state);
        }
        */

        if (Input.InteractPressed && !hasInteraction)
        {
            hasInteraction = true;

            var state = DetermineInteractionState();
            if (state != null)
                stateMachine.ChangeState(state);
        }

        /*
        if (Input.IsActionJustPressed("drop"))
            player.DropCurrentItem();
        */
    }

    private PlayerState DetermineInteractionState()
    {
        if(player.TryPickupNearby())
            return this;

        if (IsHandEmpty() && tile.CanPrepare())
            return player.PrepareGroundState;

        if (HasItemType(ItemType.Seed) && tile.CanPlant())
            return player.PlantState;
    
        return this;
    }

    private PlayerState DetermineDisruptionState()
    {
        //if (IsOnHandEmpty())
            //return player.RemoveState;

        return this;
    }
}