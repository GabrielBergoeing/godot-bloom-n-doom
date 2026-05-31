using Godot;

[GlobalClass]
public partial class ItemDatabase : Resource
{
    [Export] public ItemData[] Items;

    public ItemData GetItem(string itemId)
    {
        foreach (var item in Items)
            if (item.ItemId == itemId)
                return item;
        return null;
    }
}