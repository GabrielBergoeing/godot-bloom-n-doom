using Godot;

[GlobalClass]
public partial class SeedData : ItemData
{
    [Export] public PackedScene PlantScene;
    [Export] public Texture2D SeedTexture;
    [Export] public Texture2D GrowingTexture;
	[Export] public Texture2D MatureTexture;

    [Export] public int InteractionsToMature = 2;
    [Export] public bool BlocksMovement = false;

    //Future gameplay balance each plant restore x ammount of health
    [Export(PropertyHint.Range, "0, 25, or_greater")] public float MaxHealth = 10;
    [Export(PropertyHint.Range, "0, 100, or_greater")] public float WitheringTime = 30f;
    [Export(PropertyHint.Range, "0, 100, or_greater")] public float WitheringTickRate = 50f;

    public override void Use(ItemUseContext ctx)
    {
        var farm = FarmManager.Instance;
        if (farm == null) return;

        if (!farm.IsPrepared(ctx.Cell) || farm.IsOccupied(ctx.Cell))
            return;

        farm.PlantSeed(ctx.Cell, ctx.Player.PlayerId, this);

        var Inv = ctx.Player.Hotbar;
        Inv.RemoveItem(Inv.CurrentSlot, 1, consume: true);
    }
}