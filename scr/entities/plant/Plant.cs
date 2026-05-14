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

    private Area2D Area;
    private Sprite2D Sprite;
    private PlantHealth Health;
    private CollisionShape2D Collision;

    public override void _Ready()
    {
        Area = GetNode<Area2D>("Hurtbox");
        Sprite = GetNode<Sprite2D>("Sprite2D");
        Health = GetNode<PlantHealth>("PlantHealth");
        Collision = GetNodeOrNull<CollisionShape2D>("StaticBody2D/Collision");
    }

    public void Init(int playerIndex, Vector2I cell, SeedData seedData)
    {
        OwnerPlayerIndex = playerIndex;
        CellPos = cell;
        data = seedData;

        if (Collision != null)
            Collision.Disabled = !data.BlocksMovement;

        Area.BodyEntered += OnBodyEntered;

        currentInteractions = 0;
        Health?.Init(data);
        SetStage(GrowthStage.Seed);
    }

    public void WaterPlant()
    {
        if (Health.isOnFire)
        {
            ExtinguishFire();
            return;
        }
        
        Health.ResetWitherTimer();
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

    public void Ignite()
    {
        Health?.Ignite();
    }

    public void ExtinguishFire()
    {
        Health?.Extinguish();
    }

    public bool IsMature() => stage == GrowthStage.Mature;

    public int GetScore()
    {
        if (data == null)
            return 0;

        int score = 0;
        score += data.BaseScore;

        if (stage == GrowthStage.Growing)
            score += data.GrowingBonus;

        if (stage == GrowthStage.Mature)
            score += data.MatureBonus;

        score += GetAdjacentPlantBonus();
        return score;
    }

    private void OnBodyEntered(Node body)
    {
        if (!data.EnableWaterStorage || !IsMature())
            return;
        
        GD.Print(body);
        
        if (body is not Player player || player.PlayerId == OwnerPlayerIndex)
            return;
        
        GD.Print("Player Spotted");
        int stealAmount = Mathf.Min(
            data.WaterStealAmount,
            player.Water.CurrentWater
        );

        GD.Print("Cactus has stolen water!");
        player.Water.ConsumeWater(stealAmount);
        Health.StashWaterReserve(stealAmount);
    }

    private void SetStage(GrowthStage newStage)
    {
        stage = newStage;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (Sprite == null || data == null) return;

        Sprite.Texture = stage switch
        {
            GrowthStage.Seed => data.SeedTexture,
            GrowthStage.Growing => data.GrowingTexture,
            GrowthStage.Mature => data.MatureTexture,
            _ => Sprite.Texture
        };
    }

    private int GetAdjacentPlantBonus()
    {
        int bonus = 0;
        if (!data.EnableAdjacentMatureBonus || !IsMature())
            return bonus;

        var plants = FarmManager.Instance.GetAdjacentPlants(CellPos);
        foreach (Plant plant in plants)
        {
            if (plant.IsMature())
                bonus += data.AdjacentMatureBonus;
        }
        return bonus;
    }
}