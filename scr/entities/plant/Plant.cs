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

    private Sprite2D Sprite;
    private PlantHealth Health;
    private CollisionShape2D Collision;

    public override void _Ready()
    {
        Sprite = GetNode<Sprite2D>("Sprite2D");
        Health = GetNode<PlantHealth>("PlantHealth");
        Collision = GetNodeOrNull<CollisionShape2D>("StaticBody2D/Collision");

        var bodies = GetChildren();

        foreach (var child in bodies)
        {
            GD.Print($"Child: {child.Name} ({child.GetType()})");

            if (child is CollisionObject2D obj)
            {
                GD.Print($"Layer: {obj.CollisionLayer}");
                GD.Print($"Mask: {obj.CollisionMask}");
            }

            if (child is Area2D area)
            {
                var shape = area.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");

                GD.Print($"Area Shape Exists: {shape?.Shape != null}");
                GD.Print($"Area Monitoring: {area.Monitoring}");
                GD.Print($"Area Monitorable: {area.Monitorable}");
            }

            if (child is PhysicsBody2D body)
            {
                var shape = body.GetNodeOrNull<CollisionShape2D>("Collision");

                GD.Print($"Body Shape Exists: {shape?.Shape != null}");
                GD.Print($"Body Shape Disabled: {shape?.Disabled}");
            }
        }
    }

    public void Init(int playerIndex, Vector2I cell, SeedData seedData)
    {
        OwnerPlayerIndex = playerIndex;
        CellPos = cell;
        data = seedData;

        if (Collision != null)
            Collision.Disabled = !data.BlocksMovement;

        currentInteractions = 0;
        Health?.Init(data);
        SetStage(GrowthStage.Seed);
        GD.Print($"Plant: {Name} at {GlobalPosition}");
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

    public void WaterPlant()
    {
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
        GD.Print("iM BURNING");
    }

    public void ExtinguishFire()
    {
        Health?.Extinguish();
    }

    public bool IsMature() => stage == GrowthStage.Mature;
}