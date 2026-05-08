using Godot;

[GlobalClass]
public partial class FertilizerData : ItemData
{
    public override bool CanUse(ItemUseContext ctx)
    {
        return ctx.Tile.CanInteractPlant(ctx.PlayerId);
    }

    public override void Use(ItemUseContext ctx)
    {
        ctx.Tile.FertilizeInCell();
        ctx.Player.Hotbar.ConsumeCurrentStack();
    }
}