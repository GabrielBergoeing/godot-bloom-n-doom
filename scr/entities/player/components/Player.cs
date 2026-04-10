using Godot;

public partial class Player : Entity
{
	[Export] public float MoveSpeed = 200f;

	public Vector2 MoveInput { get; private set; }

	// States
	public PlayerIdleState IdleState { get; private set; }
	public PlayerMoveState MoveState { get; private set; }

	public override void _Ready()
	{
		base._Ready();

		IdleState = new PlayerIdleState(this, stateMachine);
		MoveState = new PlayerMoveState(this, stateMachine);

		stateMachine.Initialize(IdleState);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		MoveInput = Input.GetVector("move_left", "move_right", "move_up", "move_down");
	}

	public override void _PhysicsProcess(double delta)
	{
		MoveAndSlide();
	}
}
