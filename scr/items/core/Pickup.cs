using Godot;
using System;

public partial class Pickup : Area2D
{
    [Export] public ItemData ItemData;

    public Action<Player> OnPickup;

    public void SetItemData(ItemData data)
    {
        ItemData = data;
    }

    public void Pick(Player player)
    {
        var hotbar = player.GetNodeOrNull<PlayerHotbar>("PlayerHotbar");

        if (hotbar != null && hotbar.AddItem(ItemData))
        {
            OnPickup?.Invoke(player);
            QueueFree();
        }
    }
}