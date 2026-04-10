using Godot;

public partial class Player : Entity
{
	[Export] public float MoveSpeed = 200f;

	public Vector2 MoveInput { get; private set; }
	public Vector2 FacingDir { get; private set; } = Vector2.Down;
	public PlayerAnim anim { get; private set; }

	// States
	public PlayerIdleState IdleState { get; private set; }
	public PlayerMoveState MoveState { get; private set; }

	public override void _Ready()
	{
		base._Ready();

		IdleState = new PlayerIdleState(this, stateMachine);
		MoveState = new PlayerMoveState(this, stateMachine);
		anim = GetNode<PlayerAnim>("AnimatedSprite2D");

		stateMachine.Initialize(IdleState);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		MoveInput = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		UpdateFacingDir();
	}

	public override void _PhysicsProcess(double delta)
	{
		MoveAndSlide();
	}

	private void UpdateFacingDir()
	{
		if (MoveInput == Vector2.Zero)
			return;

		FacingDir = MoveInput.Normalized();
	}
}
