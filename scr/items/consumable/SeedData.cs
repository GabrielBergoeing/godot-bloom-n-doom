using Godot;

[GlobalClass]
public partial class SeedData : ItemData
{
    [Export] public PackedScene PlantScene;

    [ExportGroup("Plant Textures")]
    [Export] public Texture2D SeedTexture;
    [Export] public Texture2D GrowingTexture;
	[Export] public Texture2D MatureTexture;

    [ExportGroup("Plant Stats")]
    [Export] public int InteractionsToMature = 2;
    [Export] public bool BlocksMovement = false;
    [Export(PropertyHint.Range, "0, 25, or_greater")] public float MaxHealth = 10;
    [Export(PropertyHint.Range, "0, 100, or_greater")] public float WitheringTime = 30f;
    [Export(PropertyHint.Range, "0, 100, or_greater")] public float WitheringTickRate = 50f;

    [ExportGroup("Scoring")]
    [Export(PropertyHint.Range, "0,100,or_greater")]
    public int BaseScore = 1;
    [Export(PropertyHint.Range, "0,100,or_greater")]
    public int GrowingBonus = 1;
    [Export(PropertyHint.Range, "0,100,or_greater")]
    public int MatureBonus = 3;
    [Export(PropertyHint.Range, "0,100,or_greater")]
    public int HealthyBonus = 1;

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