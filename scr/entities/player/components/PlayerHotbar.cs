using Godot;

public partial class HotbarSystem : Node
{
    [Export] public int SlotCount = 4;

    private ItemStack[] slots;
    private int currentSlot = 0;
    public int CurrentSlot => currentSlot;

    private Node2D playerHand;

    public override void _Ready()
    {
        slots = new ItemStack[SlotCount];
        playerHand = GetNode<Node2D>("../OnHand");
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("slot_1")) SelectSlot(0);
        if (Input.IsActionJustPressed("slot_2")) SelectSlot(1);
        if (Input.IsActionJustPressed("slot_3")) SelectSlot(2);
        if (Input.IsActionJustPressed("slot_4")) SelectSlot(3);
    }

    public bool AddItem(ItemData data, int amount = 1)
    {
        // Try stacking
        if (data.Stackable)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null && slots[i].Data == data && !slots[i].IsFull())
                {
                    slots[i].Amount++;
                    return true;
                }
            }
        }

        // Find empty slot
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = new ItemStack(data, amount);
                return true;
            }
        }

        return false;
    }

    public void RemoveItem(int slotIndex, int amount = 1, bool consume = false)
    {
        var stack = slots[slotIndex];
        if (stack == null) return;

        stack.Amount -= amount;

        if (stack.Amount <= 0)
        {
            if (!consume)
                DropItem(stack.Data);

            slots[slotIndex] = null;
        }
        else if (!consume)
            DropItem(stack.Data);
    }

    private void SelectSlot(int index)
    {
        //if (slots[currentSlot] != null)
            //slots[currentSlot].Visible = false;

        currentSlot = index;

        //if (slots[currentSlot] != null)
        //{
            //var item = slots[currentSlot];
            
            //Attatch to Pickup script
            //item.Visible = true;
            //item.Reparent(playerHand);
            //item.Position = Vector2.Zero;
        //}
    }

    private void DropItem(ItemData data)
    {
        if (data.PickupScene == null) return;

        var pickup = data.PickupScene.Instantiate<Node2D>();
        GetTree().CurrentScene.AddChild(pickup);

        pickup.GlobalPosition = GetParent<Node2D>().GlobalPosition;

        var pickupScript = pickup as Pickup;
        pickupScript?.SetItemData(data);
    }

    public ItemStack GetCurrentStack() => slots[currentSlot];
}