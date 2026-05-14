using Godot;
using System;

public partial class PlayerWater : Node
{
    [Export] public int MaxWater = 10;
    [Export] public int CurrentWater = 10;
    [Export] public int WaterCostPerUse = 1;
    [Export] public int RefillAmount = 5;

    [Export(PropertyHint.Range, "0, 10, or_greater")] public int Duration;
    [Export(PropertyHint.Range, "0, 10, or_greater")] public int Cooldown;

    public Action OnWaterChanged;

    public bool CanWater()
    {
        return CurrentWater >= WaterCostPerUse;
    }

    public bool TryConsumeWater()
    {
        if (!CanWater())
            return false;

        CurrentWater -= WaterCostPerUse;
        OnWaterChanged?.Invoke();
        return true;
    }

    public void Refill()
    {
        CurrentWater = Mathf.Min(CurrentWater + RefillAmount, MaxWater);
        OnWaterChanged?.Invoke();
    }

    public void FullRefill()
    {
        CurrentWater = MaxWater;
        OnWaterChanged?.Invoke();
    }

    public void ConsumeWater(int ammount)
    {
        CurrentWater -= ammount;
        OnWaterChanged?.Invoke();
    }

    public float GetNormalized() =>
        MaxWater == 0 ? 0 : (float)CurrentWater / MaxWater;
}