using Godot;

[GlobalClass]
public partial class SeedData : ItemData
{
    [Export] private PackedScene plantScene;

    public void Use(Player player, Vector2I cell)
    {
        var farm = FarmManager.Instance;
        if (farm == null) return;
        
        //Vector2I cell = player.GetTargetCell(); // you define this

        if (!farm.IsPrepared(cell) || farm.IsOccupied(cell))
            return;

        farm.PlantSeed(cell, player.PlayerId);

        var hotbar = player.GetNodeOrNull<HotbarSystem>("HotbarSystem");
        hotbar?.RemoveItem(hotbar.GetCurrentSlot(), 1, consume: true);
    }
}