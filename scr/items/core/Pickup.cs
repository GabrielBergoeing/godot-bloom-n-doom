using Godot;
using System;

public partial class Pickup : Area2D
{
    [Export] public ItemData ItemData;

    public Action<Player> OnPickup;

    public override void _Ready()
    {
        var sprite = GetNodeOrNull<Sprite2D>("Sprite2D");

        if (sprite != null && ItemData?.Icon != null)
            sprite.Texture = ItemData.Icon;

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

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

    private void OnBodyEntered(Node body)
    {
        if (body is Player player)
            player.PickupsInRange.Add(this);
    }

    private void OnBodyExited(Node body)
    {
        if (body is Player player)
            player.PickupsInRange.Remove(this);
    }
}