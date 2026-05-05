using Godot;

[GlobalClass]
public partial class ScissorsData : ItemData
{
    public override bool CanUse(ItemUseContext ctx)
    {
        return ctx.CanSabotage;
    }

    public override void Use(ItemUseContext ctx)
    {
        var farm = FarmManager.Instance;
        if (farm == null) return;

        if (farm.IsOccupied(ctx.Cell))
            farm.RemovePlant(ctx.Cell);
    }
}
