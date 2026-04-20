using Godot;

public class PlayerIdleState : PlayerNeutralState
{
	public PlayerIdleState(Player player, StateMachine sm) : base(player, sm)
	{}

	public override void Update(float delta)
	{
		base.Update(delta);
		Anim.SetAction("idle");

		if (Input.MoveInput != Vector2.Zero)
		{
			stateMachine.ChangeState(player.MoveState);
		}
	}
}
