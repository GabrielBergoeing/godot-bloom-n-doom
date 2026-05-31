using Godot;
using System;

public partial class UILobbyCountdown : Label
{
    public event Action OnCountdownComplete;
    public event Action OnCountdownCancelled;

    private const float CountdownDuration = 3.0f;
    private float _timer = -1f;
    private bool _running = false;

    public bool IsRunning => _running;

    public override void _Ready()
    {
        Visible = false;
        Text = "";
    }

    public override void _Process(double delta)
    {
        if (!_running) return;

        _timer -= (float)delta;
        Text = $"Starting in {Mathf.CeilToInt(_timer)}...";

        if (_timer <= 0f)
            Complete();
    }

    public void Begin()
    {
        if (_running) return;

        _timer = CountdownDuration;
        _running = true;
        Visible = true;
        Text = $"Starting in {Mathf.CeilToInt(_timer)}...";
    }

    public void Cancel()
    {
        if (!_running) return;

        _running = false;
        _timer = -1f;
        Visible = false;
        Text = "";

        OnCountdownCancelled?.Invoke();
    }

    private void Complete()
    {
        _running = false;
        _timer = -1f;
        Visible = false;
        Text = "";

        OnCountdownComplete?.Invoke();
    }
}