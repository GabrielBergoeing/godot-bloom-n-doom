using Godot;
using System;

public partial class NetworkTickManager : Node
{
    public static NetworkTickManager Instance;

    [Export]
    public int TickRate = 20;

    public int CurrentTick { get; private set; }

    private double _tickTimer;
    private double TickInterval =>
        1.0 / TickRate;

    public event Action<int> OnNetworkTick;

    public override void _Ready()
    {
        Instance = this;

        GD.Print(
            $"[NetworkTickManager] Started at {TickRate}hz"
        );
    }

    public override void _Process(double delta)
    {
        _tickTimer += delta;

        while (_tickTimer >= TickInterval)
        {
            _tickTimer -= TickInterval;

            ExecuteTick();
        }
    }

    private void ExecuteTick()
    {
        CurrentTick++;

        OnNetworkTick?.Invoke(CurrentTick);
    }
}