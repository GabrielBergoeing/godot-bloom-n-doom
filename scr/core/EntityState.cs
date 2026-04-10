using Godot;

public abstract class EntityState
{
	protected Entity entity;
	protected StateMachine stateMachine;

	protected float stateTimer;
	protected bool triggerCalled;

	public EntityState(Entity entity, StateMachine stateMachine)
	{
		this.entity = entity;
		this.stateMachine = stateMachine;
	}

	public virtual void Enter()
	{
		triggerCalled = false;
	}

	public virtual void Update(float delta)
	{
		stateTimer -= delta;
	}

	public virtual void Exit() { }

	public virtual void AnimationTrigger()
	{
		triggerCalled = true;
	}
}
