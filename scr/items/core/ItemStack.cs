public class ItemStack
{
    public ItemData Data;
    public int Amount;

    public ItemStack(ItemData data, int amount)
    {
        Data = data;
        Amount = amount;
    }

    public bool IsFull()
    {
        return Amount >= Data.MaxStack;
    }
}