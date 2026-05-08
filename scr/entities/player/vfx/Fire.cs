using Godot;
using Godot.Collections;

public partial class Fire : Node2D
{
    private Player Player;

    private GpuParticles2D Particles;
    private Area2D Hitbox;
	private CollisionShape2D CollisionShape;

    public override void _Ready()
    {
        Particles = GetNode<GpuParticles2D>("GPUParticles2D");
        Hitbox = GetNode<Area2D>("Area2D");
		CollisionShape = Hitbox.GetNode<CollisionShape2D>("CollisionShape2D");

        Particles.Emitting = true;
    }

    public void Initialize(Player player)
    {
        Player = player;
        UpdateTransform();
    }

    public void UpdateTransform()
    {
        if (Player == null)
            return;

        Vector2 dir = Player.GetFacingDirection();

        Vector2 targetPos =
            Player.GlobalPosition + (dir * 24f);

        Hitbox.GlobalPosition = targetPos;
        Hitbox.GlobalRotation =
            dir.Angle() + Mathf.Pi / 2f;

        Particles.GlobalPosition = targetPos;
        Particles.GlobalRotation =
            Hitbox.GlobalRotation;
    }

	public void Burn()
	{
		var space = GetWorld2D().DirectSpaceState;

		var query = new PhysicsShapeQueryParameters2D
		{
			Shape = CollisionShape.Shape,
			Transform = Hitbox.GlobalTransform,
			CollisionMask = Hitbox.CollisionMask,

			CollideWithAreas = true,
			CollideWithBodies = false
		};

		var results = space.IntersectShape(query);
		foreach (var result in results)
		{
			Node collider = result["collider"].As<Node>();
			Plant plant = collider.GetParentOrNull<Plant>();

			if (plant == null)
				continue;

			plant.Ignite();
		}
	}
}