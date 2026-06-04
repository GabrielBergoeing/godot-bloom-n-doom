using Godot;

[GlobalClass]
public partial class WaterGunData : ToolData
{
    [Export] public PackedScene ProjectileScene;

    [Export(PropertyHint.Range, "0.01,1.0")] public float FireRate = 0.08f;

    private double fireTimer;

    public override bool CanUse(ItemUseContext ctx)
    {
        return ProjectileScene != null
            && ctx.Player.Water.CanWater();
    }

    public override void BeginUse(ItemUseContext ctx)
    {
        fireTimer = 0;
        //base.BeginUse(ctx);
    }

    public override void TickUse(ItemUseContext ctx, double delta)
    {
        if (delta <= 0) return;
        fireTimer -= delta;

        if (fireTimer > 0) return;
        if (!ctx.Player.Water.TryConsumeWater()) return;

        if(NetworkRoot.Instance.IsOnline)
            DetermineProjectileSpawn(ctx);
        else
            SpawnProjectile(ctx);
        fireTimer = FireRate;
    }

    private void DetermineProjectileSpawn(ItemUseContext ctx)
    {
        if (NetworkRoot.Instance.Lobby.IsHost)
            SpawnProjectileAndBroadcast(ctx);
        else
        {
            // Peer: request host to spawn — water is consumed locally
            // but projectile authority belongs to host
            Vector2 dir = ctx.Player.GetFacingDirection();
            Vector2 pos = ctx.Player.GlobalPosition + (dir * 18f);
            SteamMatchManager.Instance.BroadcastProjectileSpawn(
                ctx.PlayerId,
                ctx.Player.OwnerSteamId,
                pos,
                dir,
                ctx.Player.Velocity
            );
        }
    }

    private WaterProjectile SpawnProjectile(ItemUseContext ctx)
    {
        var level = SplitScreenManager.Instance?.LevelNode;
        if (level == null) return null;

        var projectile = ProjectileScene.Instantiate<WaterProjectile>();
        level.AddChild(projectile);

        Vector2 dir = ctx.Player.GetFacingDirection();

        projectile.GlobalPosition = ctx.Player.GlobalPosition + (dir * 18f);
        projectile.Initialize(dir, ctx.Player.Velocity);
        return projectile;
    }

    private void SpawnProjectileAndBroadcast(ItemUseContext ctx)
    {
        var projectile = SpawnProjectile(ctx);
        if (projectile == null) return;

        Vector2 dir = ctx.Player.GetFacingDirection();
        SteamMatchManager.Instance.BroadcastProjectileSpawn(
            ctx.PlayerId,
            ctx.Player.OwnerSteamId,
            projectile.GlobalPosition,
            dir,
            ctx.Player.Velocity
        );
    }
}