using Godot;

public partial class PlayerNeutralState : PlayerState
{
    private bool hasInteraction = false;

    public PlayerNeutralState(Player player, StateMachine sm)
        : base(player, sm)
    {
    }

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

        if (Input.IsActionJustPressed("pickup"))
            stateMachine.ChangeState(player.PickState);

        if (Input.IsActionJustPressed("interact") && !hasInteraction)
        {
            hasInteraction = true;

            var state = DetermineInteractionState();
            if (state != null)
                stateMachine.ChangeState(state);
        }

        if (Input.IsActionJustPressed("drop"))
            player.DropCurrentItem();
        */
    }

    private PlayerState DetermineInteractionState()
    {
        //if (IsOnHandEmpty())
            //return player.PrepareGroundState;

        return this;
    }

    private PlayerState DetermineDisruptionState()
    {
        //if (IsOnHandEmpty())
            //return player.RemoveState;

        return this;
    }
}