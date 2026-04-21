using Godot;
using System;

public partial class PlantHealth : Node
{
    private float Health;
    private float Timer;

    private Plant Plant;
    private SeedData Data;

    public override void _Ready()
    {
        Plant = GetParent<Plant>();
    }

    public void Init(SeedData seedData)
    {
        Data = seedData;

        Health = Data.MaxHealth;
        Timer = Data.WitheringTime;
    }

    public override void _Process(double delta)
    {
        if (Data == null || Plant == null)
            return;

        float d = (float)delta;
        Timer -= d;
        ModulateHealth();

        if (Timer <= 0f || Health <= 0f)
            Die();
    }

    public void ResetWitherTimer()
    {
        if (Data == null) return;

        Timer = Data.WitheringTime;
    }

    public void TakeDamage(float amount)
    {
        Health -= amount;

        if (Health <= 0f)
            Die();
    }

    public float GetHealthRatio()
    {
        if (Data == null || Data.MaxHealth <= 0)
            return 0f;

        return Mathf.Clamp(Health / Data.MaxHealth, 0f, 1f);
    }

    public float GetWitherRatio()
    {
        if (Data == null || Data.WitheringTime <= 0)
            return 0f;

        return Mathf.Clamp(Timer / Data.WitheringTime, 0f, 1f);
    }

    private void Die()
    {
        var farm = FarmManager.Instance;
        if (farm != null)
            farm.RemovePlant(Plant.CellPos);

        Plant.QueueFree();
    }

    private void ModulateHealth()
    {
        float ratio = GetWitherRatio();
        Plant.Modulate = new Color(1, ratio, ratio);
    }

}