using Godot;

public struct ItemUseContext
{
    public Player Player;
    public PlayerTileInteraction Tile;
    public int PlayerId;

    public ItemUseContext(Player player, PlayerTileInteraction tile)
    {
        Player = player;
        Tile = tile;
        PlayerId = player.PlayerId;
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
    [Export] public string SoundName;

    [Export] public bool Consumable = false;

    [Export(PropertyHint.Range, "0, 10, or_greater")] public int Duration;
    [Export(PropertyHint.Range, "0, 10, or_greater")] public int Cooldown;

    public virtual bool CanUse(ItemUseContext ctx) 
    {
        return false;
    }
    
    public virtual void Use(ItemUseContext ctx) {}
}