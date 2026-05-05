using Godot;

public struct ItemUseContext
{
    public Player Player;
    public Vector2I Cell;

    public bool CanPlant;
    public bool CanPrepare;
    public bool CanInteractPlant;
    public bool CanRefillWater;
    public bool HasWater;

    public ItemUseContext(Player player, PlayerTileInteraction tile)
    {
        Player = player;
        Cell = tile.CurrentCell;

        CanPlant = tile.CanPlant();
        CanPrepare = tile.CanPrepare();
        CanInteractPlant = tile.CanInteractPlant(player.PlayerId);
        CanRefillWater = tile.CanRefillWater();

        HasWater = player.Water?.CanWater() ?? false;
    }
}

public enum ItemType
{
    None,
    Seed,
    Tool,
    Resource
}

[GlobalClass]
public partial class ItemData : Resource
{
    [Export] public string ItemId;
    [Export] public string ItemName;
    [Export] public ItemType Type;
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