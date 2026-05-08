using Godot;

[GlobalClass]
public partial class WaterGunData : ToolData
{
    [Export] public PackedScene ProjectileScene;

    [Export(PropertyHint.Range, "0.01,1.0")]
    public float FireRate = 0.08f;

    private double fireTimer;

    public override bool CanUse(ItemUseContext ctx)
    {
        return ProjectileScene != null
            && ctx.CanWater;
    }

    public override void BeginUse(ItemUseContext ctx)
    {
        fireTimer = 0;
    }

    public override void TickUse(
        ItemUseContext ctx,
        double delta)
    {
        fireTimer -= delta;

        if (fireTimer > 0)
            return;

        if (!ctx.Player.Water.TryConsumeWater())
            return;

        SpawnProjectile(ctx);

        fireTimer = FireRate;
    }

    private void SpawnProjectile(ItemUseContext ctx)
    {
        var level = SplitScreenManager.Instance?.LevelNode;

        if (level == null)
            return;

        var projectile = ProjectileScene.Instantiate<WaterProjectile>();
        level.AddChild(projectile);

        Vector2 dir = ctx.Player.GetFacingDirection();

        projectile.GlobalPosition = ctx.Player.GlobalPosition + (dir * 18f);
        projectile.Initialize(dir, ctx.Player.Velocity);
    }
}