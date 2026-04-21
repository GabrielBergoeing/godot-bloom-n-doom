using Godot;

public partial class UIPlayerWater : Control
{
    [Export] private Texture2D FullIcon;
    [Export] private Texture2D MidIcon;
    [Export] private Texture2D EmptyIcon;

    private TextureRect Background;
    private TextureProgressBar Bar;
    private TextureRect Icon;
    private PlayerWater Water;

    public override void _Ready()
    {
        Background = GetNode<TextureRect>("Background");
        Bar = GetNode<TextureProgressBar>("Bar");
        Icon = GetNode<TextureRect>("Icon");
    }

    public void Bind(PlayerWater waterSystem)
    {
        Water = waterSystem;
        Water.OnWaterChanged += UpdateUI;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (Water == null)
            return;

        float ratio = Water.GetNormalized();

        Bar.Value = ratio * 100f;

        if (ratio > 0.7f)
            Icon.Texture = FullIcon;
        else if (ratio > 0.3f)
            Icon.Texture = MidIcon;
        else
            Icon.Texture = EmptyIcon;
    }
}