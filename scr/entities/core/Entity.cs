using Godot;

public partial class Entity : CharacterBody2D
{
	[Export] public float MoveSpeed = 200f;
	[Export] public float PushDecay = 10f;

	protected StateMachine stateMachine;

	protected Vector2 MoveVelocity = Vector2.Zero;
	protected Vector2 ExternalVelocity = Vector2.Zero;

	public override void _Ready()
	{
		stateMachine = new StateMachine();
	}

	public override void _Process(double delta)
	{
		stateMachine?.Update((float)delta);
	}

	public override void _PhysicsProcess(double delta)
	{
		ApplyExternalForces((float)delta);
	}

	public void AddForce(Vector2 force)
	{
		ExternalVelocity += force;
	}

	public void SetMoveVelocity(Vector2 velocity)
	{
		MoveVelocity = velocity;
	}


	protected void ApplyExternalForces(float delta)
	{
		Velocity = MoveVelocity + ExternalVelocity;

		ExternalVelocity =
			ExternalVelocity.Lerp(
				Vector2.Zero,
				PushDecay * delta
			);

		MoveAndSlide();
	}
}