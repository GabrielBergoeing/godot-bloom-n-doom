using Godot;
using System;

public abstract partial class PlayerState : EntityState
{
    protected Player player;

    protected PlayerAnim anim { get; private set; }
	protected PlayerTileInteraction tile { get; private set; }
    protected PlayerInput input { get; private set; }
    //protected Node sfx { get; private set; }
    protected PlayerHotbar Inventory { get; private set; }

    public PlayerState(Player player, StateMachine stateMachine)
        : base(player, stateMachine)
    {
        this.player = player;

        anim = player.GetNode<PlayerAnim>("AnimatedSprite2D");
		tile = player.Tile;
        input = player.Input;
        //sfx = player.GetNodeOrNull("Player_SFX");
        Inventory = player.Hotbar;
    }

    public override void Update(float delta)
    {
        base.Update(delta);
        Inventory.HandleInput(input);
    }

    /*
    public virtual T GetItemFromOnHand<T>() where T : Node
    {
        if (Inventory == null) 
            return null;

        var item = Inventory.GetCurrentItem();
        if (item != null)
            return item.GetNodeOrNull<T>("");

        return null;
    }
    */

    public bool IsOnHandEmpty()
    {
        var onHand = player.GetNodeOrNull<Node2D>("OnHand");
        if (onHand == null) return true;

        foreach (Node child in onHand.GetChildren())
        {
            if (child is CanvasItem canvasItem && canvasItem.Visible)
                return false;
        }

        return true;
    }
}