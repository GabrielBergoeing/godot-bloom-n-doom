using Godot;
using System;

public partial class UIPlayerTimer : Label
{
    [Export] public float UpdateInterval = 1f;
    private double _nextUpdateTime = 0f;

    //For a decouple independent system, make init function that recieves a timer variable
    private MatchManager Match => MatchManager.Instance;

    private readonly Color _startColor = new Color(1f, 1f, 1f, 0.8f);
    private readonly Color _endColor = new Color(1f, 0.2f, 0.2f, 1f);

    public override void _Process(double delta)
    {
        if (Time.GetTicksMsec() / 1000.0 >= _nextUpdateTime)
        {
            UpdateTimerDisplay();

            _nextUpdateTime =
                (Time.GetTicksMsec() / 1000.0)
                + UpdateInterval;
        }
    }

    private void UpdateTimerDisplay()
    {
        float remainingTime = Mathf.Max(Match.Timer, 0f);
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);

        Text = $"{minutes:00}:{seconds:00}";

        float t =
            Mathf.InverseLerp(
                0f,
                Match.MatchDuration,
                remainingTime
            );

        Modulate = _endColor.Lerp(_startColor, t);
    }
}