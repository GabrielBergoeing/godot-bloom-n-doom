using Godot;
using System;

public abstract partial class PlayerState : EntityState
{
    protected Player player;

    protected PlayerAnim Anim { get; private set; }
	protected PlayerTileInteraction tile { get; private set; }
    protected PlayerInput Input { get; private set; }
    //protected Node sfx { get; private set; }
    protected PlayerHotbar Inventory { get; private set; }

    public PlayerState(Player player, StateMachine stateMachine)
        : base(player, stateMachine)
    {
        this.player = player;

        Anim = player.GetNode<PlayerAnim>("AnimatedSprite2D");
		tile = player.Tile;
        Input = player.Input;
        //sfx = player.GetNodeOrNull("Player_SFX");
        Inventory = player.Hotbar;
    }

    public override void Update(float delta)
    {
        base.Update(delta);
        Inventory.HandleInput(Input);
    }

    public bool HasItemType(ItemType type)
    {
        var stack = Inventory.GetCurrentStack();
        return stack?.Data?.Type == type;
    }

    public bool IsHandEmpty()
    {
        var stack = Inventory.GetCurrentStack();
        return stack == null || stack?.Data == null;
    }

    protected bool TryPickupNearby()
	{
		var pickup = player.GetPickupNearby();
		if (pickup == null) 
            return false;

		if (!Inventory.CanAddItem(pickup.ItemData))
			return false;

		pickup.Pick(player);
		return true;
	}

	protected bool TryDroppingItem()
	{
        if (IsHandEmpty())
            return false;
        
		Inventory.DropCurrentItem();
        return true;
	}
}