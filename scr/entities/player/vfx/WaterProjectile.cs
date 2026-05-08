using Godot;

public partial class WaterProjectile : Node2D
{
    [Export] public float Speed = 250f;
    [Export] public float Lifetime = 0.3f;
    [Export] public float SpreadAngle = 15f;

    [Export] public float PushForce = 250f;

    private Area2D Hitbox;
    private Timer LifetimeTimer;

    private Vector2 direction;
    private Vector2 inheritedVelocity;

    public override void _Ready()
    {
        Hitbox = GetNode<Area2D>("Area2D");

        Hitbox.AreaEntered += OnAreaEntered;
        Hitbox.BodyEntered += OnBodyEntered;

        LifetimeTimer = new Timer();
        LifetimeTimer.OneShot = true;
        LifetimeTimer.WaitTime = Lifetime;

        AddChild(LifetimeTimer);

        LifetimeTimer.Timeout += QueueFree;
        LifetimeTimer.Start();
    }

    public void Initialize(
        Vector2 shootDirection,
        Vector2 inheritedVel)
    {
        inheritedVelocity = inheritedVel;

        float spread = (float) Mathf.DegToRad(
                GD.RandRange(-SpreadAngle, SpreadAngle));

        direction = shootDirection.Rotated(spread).Normalized();

        Rotation = direction.Angle() + Mathf.Pi / 2f;
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity =
            (direction * Speed) + inheritedVelocity;

        GlobalPosition += velocity * (float)delta;
    }

    private void OnAreaEntered(Area2D area)
    {
        Plant plant = area.GetParentOrNull<Plant>();

        if (plant == null)
            return;

        plant.ExtinguishFire();
        plant.WaterPlant();
    }
    
    private void OnBodyEntered(Node2D body)
    {
        if (body is not Player player)
            return;

        Vector2 pushDir = (player.GlobalPosition - GlobalPosition).Normalized();
        player.AddForce(pushDir * PushForce);
    }
}