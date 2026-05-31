using Godot;
using System;

public partial class Pickup : Area2D
{
    [Export] public ItemData ItemData;

    public Action<Player> OnPickup;
    public int NetworkPickupId { get; set; } = -1;

    private Sprite2D _sprite;

    public override void _Ready()
    {
        _sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
        UpdateVisual();

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    public void SetItemData(ItemData data)
    {
        ItemData = data;
        UpdateVisual();
    }

    public ItemData GetItemData() => ItemData;

    public void Pick(Player player)
    {
        GD.Print("[Pickup] Picking item");
        if (NetworkRoot.Instance.IsOnline)
        {
            if (NetworkRoot.Instance.Lobby.IsHost)
                PickupNetworkService.Instance.CollectPickup(NetworkPickupId, player);
            else if (player.IsLocallyControlled)
            {
                SteamMatchManager.Instance.RequestPickupCollect(NetworkPickupId, player.OwnerSteamId);
                GD.Print("[Pickup] Requested to pick item");
            }
        }
        else
        {
            if (player.Hotbar.AddItem(ItemData))
            {
                player.PickupsInRange.Remove(this);
                OnPickup?.Invoke(player);
                QueueFree();
            }
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
        if (_sprite == null) return;
        _sprite.Texture = ItemData?.Icon;
        _sprite.Visible = ItemData?.Icon != null;
    }
}