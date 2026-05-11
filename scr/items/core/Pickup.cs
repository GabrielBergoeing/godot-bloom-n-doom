using Godot;
using System;

public partial class Pickup : Area2D
{
    [Export] public ItemData ItemData;

    public Action<Player> OnPickup;
    private Sprite2D Sprite;

    public override void _Ready()
    {
        Sprite = GetNodeOrNull<Sprite2D>("Sprite2D");

        if (Sprite != null && ItemData?.Icon != null)
            Sprite.Texture = ItemData.Icon;

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    public void SetItemData(ItemData data)
    {
        ItemData = data;
        UpdateVisual();
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


    private void UpdateVisual()
    {
        if (Sprite == null) return;

        if (ItemData?.Icon != null)
        {
            Sprite.Texture = ItemData.Icon;
            Sprite.Visible = true;
        }
        else
            Sprite.Visible = false;
    }
}