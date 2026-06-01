using Godot;

[GlobalClass]
public partial class ItemDatabase : Resource
{
    public static ItemDatabase Instance { get; private set; }

    [Export] public ItemData[] Items;

    public static void Register(ItemDatabase db)
    {
        Instance = db;

        GD.Print($"[ItemDatabase] Registered {db.Items.Count} items");

        foreach (var item in db.Items)
            GD.Print($"[ItemDatabase] Item: {item.ItemId}");
    }

    public ItemData GetItem(string itemId)
    {
        GD.Print($"[ItemDB] Lookup '{itemId}'");

        foreach (var item in Items)
        {
            GD.Print($"[ItemDB] Candidate '{item.ItemId}'");

            if (item.ItemId == itemId)
            {
                GD.Print($"[ItemDB] FOUND '{itemId}'");
                return item;
            }
        }

        GD.PrintErr($"[ItemDB] NOT FOUND '{itemId}'");
        return null;
    }
}