using Godot;

public partial class Entity : CharacterBody2D
{
	public AnimationTree Anim { get; private set; }
	protected StateMachine stateMachine;

	public override void _Ready()
	{
		Anim = GetNode<AnimationTree>("AnimationTree");
		stateMachine = new StateMachine();
	}

	public override void _Process(double delta)
	{
		stateMachine?.Update((float)delta);
	}

	public virtual void SetVelocity(Vector2 velocity)
	{
		Velocity = velocity;
	}
}
