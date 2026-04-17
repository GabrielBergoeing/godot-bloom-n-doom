using Godot;

public class PlayerMoveState : PlayerNeutralState
{
	public PlayerMoveState(Player player, StateMachine sm) : base(player, sm)
	{}

	public override void Update(float delta)
	{
		base.Update(delta);
		anim.SetAction("move");

		if (input.MoveInput == Vector2.Zero)
		{
			stateMachine.ChangeState(player.IdleState);
			return;
		}
		
		player.SetVelocity(input.MoveInput * player.MoveSpeed);
	}

	public override void Exit()
	{
		player.SetVelocity(Vector2.Zero);
	}
}
