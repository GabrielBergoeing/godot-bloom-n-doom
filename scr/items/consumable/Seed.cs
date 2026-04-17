using Godot;

[GlobalClass]
public partial class SeedData : ItemData
{
    [Export] private PackedScene plantScene;

    public override void Use(ItemUseContext ctx)
    {
        var farm = FarmManager.Instance;
        if (farm == null) return;

        if (!farm.IsPrepared(ctx.Cell) || farm.IsOccupied(ctx.Cell))
            return;

        farm.PlantSeed(ctx.Cell, ctx.Player.PlayerId);

        var hotbar = ctx.Player.GetNodeOrNull<HotbarSystem>("HotbarSystem");
        hotbar?.RemoveItem(hotbar.CurrentSlot, 1, consume: true);
    }
}