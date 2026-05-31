using Godot;

[GlobalClass]
public partial class ItemDatabase : Resource
{
    public static ItemDatabase Instance { get; private set; }

    [Export] public ItemData[] Items;

    public static void Register(ItemDatabase db) => Instance = db;

    public ItemData GetItem(string itemId)
    {
        foreach (var item in Items)
            if (item.ItemId == itemId)
                return item;
        return null;
    }
}