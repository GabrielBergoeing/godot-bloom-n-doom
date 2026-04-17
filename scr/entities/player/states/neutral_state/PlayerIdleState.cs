using Godot;

public class PlayerIdleState : PlayerNeutralState
{
	public PlayerIdleState(Player player, StateMachine sm) : base(player, sm)
	{}

	public override void Update(float delta)
	{
		base.Update(delta);
		anim.SetAction("idle");

		if (input.MoveInput != Vector2.Zero)
		{
			stateMachine.ChangeState(player.MoveState);
		}
	}
}
