using Godot;
using System;

public partial class UIPlayerTimer : Label
{
    [Export] public float UpdateInterval = 1f;
    private double nextUpdateTime = 0f;

    //For a decouple independent system, make init function that recieves a timer variable
    private MatchManager matchManager;

    private readonly Color startColor = new Color(1f, 1f, 1f, 0.8f);
    private readonly Color endColor = new Color(1f, 0.2f, 0.2f, 1f);

    public override void _Ready()
    {
        matchManager = MatchManager.Instance;
    }

    public override void _Process(double delta)
    {
        if (Time.GetTicksMsec() / 1000.0 >= nextUpdateTime)
        {
            UpdateTimerDisplay();

            nextUpdateTime =
                (Time.GetTicksMsec() / 1000.0)
                + UpdateInterval;
        }
    }

    private void UpdateTimerDisplay()
    {
        float remainingTime = Mathf.Max(matchManager.Timer, 0f);
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);

        Text = $"{minutes:00}:{seconds:00}";

        float t =
            Mathf.InverseLerp(
                0f,
                matchManager.MatchDuration,
                remainingTime
            );

        Modulate = endColor.Lerp(startColor, t);
    }
}