using Godot;

public class PlayerMoveState : EntityState
{
	private Player player;

	public PlayerMoveState(Player player, StateMachine sm) : base(player, sm)
	{
		this.player = player;
	}

	public override void Update(float delta)
	{
		base.Update(delta);

		if (player.MoveInput == Vector2.Zero)
		{
			stateMachine.ChangeState(player.IdleState);
			return;
		}

		player.Velocity = player.MoveInput.Normalized() * player.MoveSpeed;
	}
}
