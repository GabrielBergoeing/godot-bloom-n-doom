using Godot;

[GlobalClass]
public partial class WaterGunData : ItemData
{
    [Export] public PackedScene ProjectileScene;

    public override bool CanUse(ItemUseContext ctx)
    {
        return true; // or add constraints later
    }

    public override void Use(ItemUseContext ctx)
    {
        if (ProjectileScene == null) return;

        var projectile = ProjectileScene.Instantiate<Node2D>();

        var level = SplitScreenManager.Instance.LevelNode;
        level.AddChild(projectile);

        projectile.GlobalPosition = ctx.Player.GlobalPosition;

        // optional: pass direction / velocity
    }
}