using Godot;

[GlobalClass]
public partial class ItemDatabase : Resource
{
    public static ItemDatabase Instance { get; private set; }

    [Export] public ItemData[] Items;

    public static void Register(ItemDatabase db)
    {
        Instance = db;

        foreach (ItemData item in db.Items)
        {
            if (item is ToolData tool)
            {
                tool.OnBeginToolUse -= Instance.HandleLocalToolBeginUse;
                tool.OnBeginToolUse += Instance.HandleLocalToolBeginUse;

                tool.OnEndToolUse -= Instance.HandleLocalToolEndUse;
                tool.OnEndToolUse += Instance.HandleLocalToolEndUse;
            }
        }
    }

    private void HandleLocalToolBeginUse(ItemUseContext ctx)
    {
        if (!ctx.Player.IsLocallyControlled) return;
        int slot = ctx.Player.Hotbar.CurrentSlot;

        SteamMatchManager.Instance.BroadcastToolBeginUse(
            ctx.Player.PlayerId,
            slot,
            ctx.Player.OwnerSteamId
        );
    }

    private void HandleLocalToolEndUse(ItemUseContext ctx)
    {
        if (!ctx.Player.IsLocallyControlled) return;
        int slot = ctx.Player.Hotbar.CurrentSlot;

        SteamMatchManager.Instance.BroadcastToolEndUse(
            ctx.Player.PlayerId,
            slot,
            ctx.Player.OwnerSteamId
        );
    }

    public ItemData GetItem(string itemId)
    {
        foreach (var item in Items)
            if (item.ItemId == itemId)
                return item;

        GD.PrintErr($"[ItemDatabase] Item not found: '{itemId}'");
        return null;
    }
}