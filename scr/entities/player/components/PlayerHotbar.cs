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

        int removeAmount = Mathf.Min(amount, stack.Amount);

        for (int i = 0; i < removeAmount; i++)
        {
            if (!consume)
                Player.SpawnPickup(stack.Data);
        }

        stack.Amount -= removeAmount;

        if (stack.Amount <= 0)
            slots[slotIndex] = null;

        OnSlotChanged?.Invoke();
    }

    public ItemStack GetStackAt(int index)
    {
        if (index < 0 || index >= slots.Length)
            return null;

        return slots[index];
    }

    public void DropCurrentItem()
    {
        var stack = GetCurrentStack();
        if (stack == null) return;

        Player.SpawnPickup(stack.Data);

        stack.Amount--;

        if (stack.Amount <= 0)
            slots[currentSlot] = null;

        OnSlotChanged?.Invoke();
    }

    public ItemStack GetCurrentStack()
    {
        if (slots == null || currentSlot < 0 || currentSlot >= slots.Length)
            return null;

        return slots[currentSlot];
    }

    public void ConsumeCurrentStack()
    {
        RemoveItem(currentSlot, 1, consume: true);
    }

    private void SelectSlot(int index)
    {
        currentSlot = index;
        OnSlotChanged?.Invoke();
    }
}