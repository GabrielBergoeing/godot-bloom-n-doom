using Godot;

public partial class Entity : CharacterBody2D
{
	[Export] public float MoveSpeed = 200f;
	protected StateMachine stateMachine;

	public override void _Ready()
	{
		stateMachine = new StateMachine();
	}

	public override void _Process(double delta)
	{
		stateMachine?.Update((float)delta);
	}
}
