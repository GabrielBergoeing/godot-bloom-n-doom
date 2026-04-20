using Godot;

public partial class Plant : Node2D
{
    public enum GrowthStage
    {
        Seed,
        Growing,
        Mature
    }

    private SeedData data;

    private int currentInteractions = 0;
    private GrowthStage stage = GrowthStage.Seed;

    public int OwnerPlayerIndex = -1;
    public Vector2I CellPos;

    private Sprite2D sprite;
    private CollisionShape2D collision;

    public override void _Ready()
    {
        sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
        collision = GetNodeOrNull<CollisionShape2D>("Collision");
    }

    public void Init(int playerIndex, Vector2I cell, SeedData seedData)
    {
        OwnerPlayerIndex = playerIndex;
        CellPos = cell;
        data = seedData;

        currentInteractions = 0;

        if (collision != null)
            collision.Disabled = !data.BlocksMovement;

        SetStage(GrowthStage.Seed);
    }

    private void SetStage(GrowthStage newStage)
    {
        stage = newStage;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (sprite == null || data == null) return;

        sprite.Texture = stage switch
        {
            GrowthStage.Seed => data.SeedTexture,
            GrowthStage.Growing => data.GrowingTexture,
            GrowthStage.Mature => data.MatureTexture,
            _ => sprite.Texture
        };
    }

    public void WaterPlant()
    {
        if (stage == GrowthStage.Mature)
            return;

        currentInteractions++;

        if (currentInteractions >= data.InteractionsToMature)
            SetStage(GrowthStage.Mature);
        else if (stage == GrowthStage.Seed)
            SetStage(GrowthStage.Growing);
    }

    public void FertilizePlant()
    {
        if (stage == GrowthStage.Mature)
            return;

        currentInteractions = data.InteractionsToMature;
        SetStage(GrowthStage.Mature);
    }

    public bool IsMature() => stage == GrowthStage.Mature;
}