using Godot;

[GlobalClass]
public partial class FlamethrowerData : ToolData
{
    [Export] public PackedScene FirePrefab;

    [Export(PropertyHint.Range, "0.01,1.0")]
    public float TickRate = 0.08f;

    private Fire currentFire;
    private double tickTimer;

    public override bool CanUse(ItemUseContext ctx)
    {
        return FirePrefab != null;
    }

    public override void BeginUse(ItemUseContext ctx)
    {
        if (currentFire != null)
            return;

        var level = SplitScreenManager.Instance?.LevelNode;
        if (level == null)
            return;

        currentFire = FirePrefab.Instantiate<Fire>();

        level.AddChild(currentFire);
        currentFire.Initialize(ctx.Player);

        tickTimer = 0;
    }

    public override void TickUse(
        ItemUseContext ctx,
        double delta)
    {
        if (currentFire == null)
            return;

        currentFire.UpdateTransform();

        tickTimer -= delta;

        if (tickTimer > 0)
            return;

        currentFire.Burn();
        tickTimer = TickRate;
    }

    public override void EndUse(ItemUseContext ctx)
    {
        if (currentFire == null)
            return;

        currentFire.QueueFree();
        currentFire = null;
    }
}