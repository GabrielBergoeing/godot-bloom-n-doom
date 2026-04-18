using Godot;
using System;

public partial class PlayerHotbar : Node
{
    public Action OnSlotChanged;

    [Export] public int SlotCount = 4;

    private ItemStack[] slots;
    private int currentSlot = 0;
    public int CurrentSlot => currentSlot;

    public Player Player { get; private set; }

    public override void _Ready()
    {
        slots = new ItemStack[SlotCount];
        Player = GetParent<Player>();
    }

    public void HandleInput(PlayerInput input)
    {
        if (input.SlotPressed.HasValue)
            SelectSlot(input.SlotPressed.Value);
    }

    public bool CanAddItem(ItemData data, int amount = 1)
    {
        // Check stacking
        if (data.Stackable)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null && slots[i].Data == data && !slots[i].IsFull())
                    return true;
            }
        }

        // Check empty slot
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
                return true;
        }

        return false;
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
                SelectSlot(i);
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
        currentSlot = index;
        OnSlotChanged?.Invoke();
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