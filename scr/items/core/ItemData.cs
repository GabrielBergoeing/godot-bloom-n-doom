using Godot;

public struct ItemUseContext
{
    public Player Player;
    public Vector2I Cell;

    public bool CanPlant;
    public bool CanWater;
    public bool CanSabotage;
    public bool CanInteractPlant;

    public ItemUseContext(Player player, PlayerTileInteraction tile)
    {
        Player = player;
        Cell = tile.CurrentCell;

        CanPlant = tile.CanPlant();
        CanWater = Player.Water.CanWater();
        CanSabotage = tile.CanSabotage(player.PlayerId);
        CanInteractPlant = tile.CanInteractPlant(player.PlayerId);
    }
}

[GlobalClass]
public partial class ItemData : Resource
{
    [Export] public string ItemId;
    [Export] public string ItemName;
    [Export] public Texture2D Icon;

    [Export] public bool Stackable = true;
    [Export] public int MaxStack = 5;

    [Export] public PackedScene PickupScene;

    [Export] public bool Consumable = false;

    [Export(PropertyHint.Range, "0, 10, or_greater")] public int Duration;
    [Export(PropertyHint.Range, "0, 10, or_greater")] public int Cooldown;

    public virtual bool CanUse(ItemUseContext ctx) 
    {
        return false;
    }
    
    public virtual void Use(ItemUseContext ctx) {}
}