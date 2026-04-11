using Godot;

public partial class Player : Entity
{
	[Export] public float MoveSpeed = 200f;

	public Vector2 MoveInput { get; private set; }
	public Vector2 FacingDir { get; private set; } = Vector2.Down;

	public PlayerAnim anim { get; private set; }
	public PlayerInput input {get; private set;}

	public int PlayerId { get; private set; }

	// States
	public PlayerIdleState IdleState { get; private set; }
	public PlayerMoveState MoveState { get; private set; }

	public override void _Ready()
	{
		base._Ready();

		IdleState = new PlayerIdleState(this, stateMachine);
		MoveState = new PlayerMoveState(this, stateMachine);
		anim = GetNode<PlayerAnim>("AnimatedSprite2D");
		input = GetNode<PlayerInput>("PlayerInput");
		
		stateMachine.Initialize(IdleState);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		MoveInput = input.MoveInput;
		UpdateFacingDir();
	}

	public override void _PhysicsProcess(double delta)
	{
		MoveAndSlide();
	}

	public void AssignDevice(int deviceId, string deviceType, int playerId)
	{
		PlayerId = playerId;

		input.AssignDevice(deviceId, deviceType, playerId);

		GD.Print($"Player {PlayerId} initialized with {deviceType} ({deviceId})");
	}

	private void UpdateFacingDir()
	{
		if (MoveInput == Vector2.Zero)
			return;

		FacingDir = MoveInput.Normalized();
	}
}
