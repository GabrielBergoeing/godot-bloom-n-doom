using Godot;

public class PlayerIdleState : EntityState
{
	private Player player;

	public PlayerIdleState(Player player, StateMachine sm) : base(player, sm)
	{
		this.player = player;
	}

	public override void Update(float delta)
	{
		base.Update(delta);

		if (player.MoveInput != Vector2.Zero)
		{
			stateMachine.ChangeState(player.MoveState);
		}
	}
}
