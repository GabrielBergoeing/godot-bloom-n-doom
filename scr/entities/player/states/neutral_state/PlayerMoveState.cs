using Godot;

public class PlayerMoveState : PlayerNeutralState
{
	public PlayerMoveState(Player player, StateMachine sm) : base(player, sm)
	{}

	public override void Update(float delta)
	{
		base.Update(delta);
		player.anim.SetAction("move");

		if (player.MoveInput == Vector2.Zero)
		{
			stateMachine.ChangeState(player.IdleState);
			return;
		}
		
		player.SetVelocity(player.MoveInput.Normalized() * player.MoveSpeed);
	}

	public override void Exit()
	{
		player.SetVelocity(Vector2.Zero);
	}
}
