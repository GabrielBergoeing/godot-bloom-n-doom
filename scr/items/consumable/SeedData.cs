using Godot;

[Tool]
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
    [Export(PropertyHint.Range, "0,100,or_greater")] public int BaseScore = 1;
    [Export(PropertyHint.Range, "0,100,or_greater")] public int GrowingBonus = 1;
    [Export(PropertyHint.Range, "0,100,or_greater")] public int MatureBonus = 3;

    [ExportGroup("Crown Flower")]
    private bool enableAdjacentMatureBonus;
    [Export] public bool EnableAdjacentMatureBonus
    {
        get => enableAdjacentMatureBonus;
        set
        {
            enableAdjacentMatureBonus = value;

            if (Engine.IsEditorHint())
                NotifyPropertyListChanged();
        }
    }
    [Export(PropertyHint.Range, "0,10,or_greater")] public int AdjacentMatureBonus = 1;

    [ExportGroup("Cactus")]
    private bool enableWaterStorage;
    [Export] public bool EnableWaterStorage
    {
        get => enableWaterStorage;
        set
        {
            enableWaterStorage = value;

            if (Engine.IsEditorHint())
                NotifyPropertyListChanged();
        }
    }
    [Export(PropertyHint.Range, "0,100,or_greater")] public int MaxWaterStorage = 10;
    [Export(PropertyHint.Range, "0,10,or_greater")] public float WaterConsumptionRate = 0.05f;
    [Export(PropertyHint.Range, "0,50,or_greater")] public int WaterStealAmount = 1;

    public override void _ValidateProperty(Godot.Collections.Dictionary property)
    {
        string name = property["name"].AsString();
        if (name == nameof(AdjacentMatureBonus))
        {
            property["usage"] = (int)(EnableAdjacentMatureBonus
                ? PropertyUsageFlags.Default
                : PropertyUsageFlags.NoEditor
            );
        }
        if (name == nameof(MaxWaterStorage) || name == nameof(WaterConsumptionRate) || name == nameof(WaterStealAmount))
        {
            property["usage"] = (int)(EnableWaterStorage
                ? PropertyUsageFlags.Default
                : PropertyUsageFlags.NoEditor
            );
        }
    }

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