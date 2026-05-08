using Godot;
using System;

public partial class PlantHealth : Node
{
    private Plant Plant;
    private SeedData Data;
    private Timer TickTimer;

    private float currentHealth;
    private float witherTimeRemaining;

    private bool isOnFire = false;
    private float fireDps = 1f;

    public override void _Ready()
    {
        Plant = GetParent<Plant>();
        CreateTimer();
    }

    public override void _Process(double delta)
    {
        if (Data == null || Plant == null)
            return;

        witherTimeRemaining -= (float)delta;

        ModulateHealth();

        if (witherTimeRemaining <= 0f || currentHealth <= 0f)
            Die();
    }

    public void Init(SeedData seedData)
    {
        Data = seedData;

        currentHealth = Data.MaxHealth;
        witherTimeRemaining = Data.WitheringTime;

        TickTimer.WaitTime = Data.WitheringTickRate;
        TickTimer.Start();
    }

    public void ResetWitherTimer()
    {
        if (Data == null)
            return;

        witherTimeRemaining = Data.WitheringTime;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0f)
            Die();
    }

    public float GetWitherRatio()
    {
        if (Data == null || Data.WitheringTime <= 0)
            return 0f;

        return Mathf.Clamp(witherTimeRemaining / Data.WitheringTime, 0f, 1f);
    }

    public void Ignite()
    {
        isOnFire = true;
    }

    public void Extinguish()
    {
        isOnFire = false;
        Plant.Modulate = Colors.White;
    }

    private void CreateTimer()
    {
        TickTimer = new Timer();
        TickTimer.OneShot = false;
        TickTimer.Autostart = false;

        AddChild(TickTimer);
        TickTimer.Timeout += OnTick;
    }

    private void OnTick()
    {
        if (Data == null || Plant == null)
            return;

        if (isOnFire)
            currentHealth -= fireDps * Data.WitheringTickRate;

        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        TickTimer.Stop();

        var farm = FarmManager.Instance;
        if (farm != null)
            farm.RemovePlant(Plant.CellPos);

        Plant.QueueFree();
    }

    private void ModulateHealth()
    {
        float ratio = GetWitherRatio();

        Color baseColor =
            Colors.White.Lerp(Colors.SaddleBrown, 1f - ratio);

        if (isOnFire)
        {
            float flicker =
                Mathf.Abs(Mathf.Sin(
                    (float)Time.GetTicksMsec() * 0.02f));

            baseColor =
                baseColor.Lerp(Colors.OrangeRed, flicker);
        }

        Plant.Modulate = baseColor;
    }
}