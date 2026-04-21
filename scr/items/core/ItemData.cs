using Godot;

public struct ItemUseContext
{
    public Player Player;
    public Vector2I Cell;

    public ItemUseContext(Player player, Vector2I cell)
    {
        Player = player;
        Cell = cell;
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

    public virtual void Use(ItemUseContext ctx) {}
}