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

    public override bool CanUse(ItemUseContext ctx)
    {
        return ctx.Tile.CanPlant();
    }

    public override void Use(ItemUseContext ctx)
    {
        ctx.Tile.PlantInCell(ctx.PlayerId, this);
        ctx.Player.Hotbar.ConsumeCurrentStack();
    }
}