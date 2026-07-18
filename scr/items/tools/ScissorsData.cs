using Godot;

[GlobalClass]
public partial class ScissorsData : ItemData
{
    public override bool CanUse(ItemUseContext ctx)
    {
        return ctx.Tile.CanSabotage(ctx.PlayerId);
    }

    public override void Use(ItemUseContext ctx)
    {
        if (NetworkRoot.Instance.IsOnline)
            FarmNetworkService.Instance.RequestSabotagePlant(ctx.Tile.CurrentCell, ctx.PlayerId);
        else
            FarmManager.Instance.RemovePlant(ctx.Tile.CurrentCell);
        ctx.Player.Hotbar.ConsumeCurrentStack();
    }
}
