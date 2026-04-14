using Godot;
using System;

public abstract partial class PlayerState : EntityState
{
    protected Player player;

    //protected Node sfx;
    protected Node tile;
    //protected Node inventory;

    public PlayerState(Player player, StateMachine stateMachine)
        : base(player, stateMachine)
    {
        this.player = player;

        //sfx = player.GetNodeOrNull("Player_SFX");
        tile = player.GetNodeOrNull("TileInteraction");
        //inventory = player.GetNodeOrNull("HotbarSystem");
    }


    public virtual void UpdateAnimation()
    {
        if (player.MoveInput == Vector2.Zero)
            return;

        //player.Anim.FlipH = player.MoveInput.X < 0;
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