using Godot;

public class PlayerMoveState : PlayerNeutralState
{
	public PlayerMoveState(Player player, StateMachine sm) : base(player, sm)
	{}

	public override void Update(float delta)
	{
		base.Update(delta);
		Anim.SetAction("move");
		SFX.PlayOnWalk();

		if (Input.MoveInput == Vector2.Zero)
		{
			stateMachine.ChangeState(player.IdleState);
			return;
		}
		
		player.SetMoveVelocity(Input.MoveInput * player.MoveSpeed);
	}

	public override void Exit()
	{
		player.SetMoveVelocity(Vector2.Zero);
	}
}
