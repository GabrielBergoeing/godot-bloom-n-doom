using Godot;
using System.Threading.Tasks;

public partial class UIFadeScreen : CanvasLayer
{
    [Export] private ColorRect _fadeRect;
    private Tween _fadeTween;

    public override void _Ready()
    {
        Color color = _fadeRect.Color;
        color.A = 1f;

        _fadeRect.Color = color;
    }

    private async Task FadeEffect(
        float targetAlpha,
        float duration)
    {
        _fadeTween?.Kill();

        _fadeTween = CreateTween();

        _fadeTween.TweenProperty(
            _fadeRect,
            "color:a",
            targetAlpha,
            duration);

        await ToSignal(
            _fadeTween,
            Tween.SignalName.Finished);
    }

    public async Task FadeIn(float duration = 1f)
    {
        Color color = _fadeRect.Color;
        color.A = 1f;

        _fadeRect.Color = color;
        await FadeEffect(0f, duration);
    }

    public async Task FadeOut(float duration = 1f)
    {
        Color color = _fadeRect.Color;
        color.A = 0f;

        _fadeRect.Color = color;
        await FadeEffect(1f, duration);
    }
}