using Godot;

[GlobalClass]
public partial class WaterGunData : ToolData
{
    [Export] public PackedScene ProjectileScene;

    public override bool CanUse(ItemUseContext ctx)
    {
        return ctx.CanWater;
    }

}