using Godot;
using System;

public partial class PlantHealth : Node
{
    private Plant Plant;
    private SeedData Data;
    private Timer TickTimer;

    private float Health;
    private float Timer;
    private bool isOnFire = false;
    private float fireDps = 1f;

    public override void _Ready()
    {
        Plant = GetParent<Plant>();
        CreateTimer();
    }

    public void Init(SeedData seedData)
    {
        Data = seedData;
        Health = Data.MaxHealth;
        Timer = Data.WitheringTickRate;

        TickTimer.WaitTime = Data.WitheringTickRate;
        TickTimer.Start();
    }

    public void ResetWitherTimer()
    {
        if (Data == null) 
            return;
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
        
        //if (isOnFire)
            //Health -= fireDps * Data.WitheringTickRate;

        Timer -= Data.WitheringTickRate;
        ModulateHealth();

        if (Timer <= 0f || Health <= 0f)
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
        Plant.Modulate = new Color(1f, ratio, ratio);
    }
}