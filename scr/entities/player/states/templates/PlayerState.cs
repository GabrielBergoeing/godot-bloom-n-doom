using Godot;
using System;

public abstract partial class PlayerState : EntityState
{
    protected Player player;

    protected PlayerAnim anim { get; private set; }
	protected PlayerTileInteraction tile { get; private set; }
    protected PlayerInput input { get; private set; }
    //protected Node sfx { get; private set; }
    protected Node inventory { get; private set; }

    public PlayerState(Player player, StateMachine stateMachine)
        : base(player, stateMachine)
    {
        this.player = player;

        anim = player.GetNode<PlayerAnim>("AnimatedSprite2D");
		tile = player.GetNode<PlayerTileInteraction>("PlayerTileInteraction");
        input = player.input;
        //sfx = player.GetNodeOrNull("Player_SFX");
        inventory = player.GetNodeOrNull("HotbarSystem");
    }

    /*
    public virtual T GetItemFromOnHand<T>() where T : Node
    {
        if (player.Inventory == null) return null;

        var item = player.Inventory.GetCurrentItem();
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