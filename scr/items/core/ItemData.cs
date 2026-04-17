using Godot;

public struct ItemUseContext
{
    public Player Player;
    public Vector2I Cell;
}

[GlobalClass]
public partial class ItemData : Resource
{
    [Export] public string ItemName;
    [Export] public Texture2D Icon;

    [Export] public bool Stackable = true;
    [Export] public int MaxStack = 5;

    [Export] public PackedScene PickupScene;

    [Export] public bool Consumable = false;

    public virtual void Use(ItemUseContext ctx) {}
}