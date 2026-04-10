using Godot;

public class PlayerMoveState : PlayerNeutralState
{
	private Player player;

	public PlayerMoveState(Player player, StateMachine sm) : base(player, sm)
	{
		this.player = player;
	}

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
