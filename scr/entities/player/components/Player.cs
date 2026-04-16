using Godot;

public partial class Player : Entity
{
	[Export] public float MoveSpeed = 200f;

	public Vector2 MoveInput { get; private set; }
	public Vector2 FacingDir { get; private set; } = Vector2.Down;

	public PlayerInput input { get; private set; }
	public int PlayerId { get; private set; }

	// States
	public PlayerIdleState IdleState { get; private set; }
	public PlayerMoveState MoveState { get; private set; }
	public PlayerPrepareGroundState PrepareGroundState { get; private set; }

	public override void _Ready()
	{
		base._Ready();
		AddToGroup("players");

		input = GetNode<PlayerInput>("PlayerInput");

		PreparePlayerState();
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
	}

	private void PreparePlayerState()
	{
		IdleState = new PlayerIdleState(this, stateMachine);
		MoveState = new PlayerMoveState(this, stateMachine);
		PrepareGroundState = new PlayerPrepareGroundState(this, stateMachine);
	}

	private void UpdateFacingDir()
	{
		if (MoveInput == Vector2.Zero)
			return;

		FacingDir = MoveInput.Normalized();
	}
}
