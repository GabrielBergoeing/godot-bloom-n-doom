using Godot;

public partial class UIPlayerWater : Control
{
    [Export] public Texture2D FullIcon;
    [Export] public Texture2D MidIcon;
    [Export] public Texture2D EmptyIcon;

    private TextureRect _background;
    private TextureProgressBar _bar;
    private TextureRect _icon;
    private PlayerWater _water;

    public override void _Ready()
    {
        _background = GetNode<TextureRect>("Background");
        _bar = GetNode<TextureProgressBar>("Bar");
        _icon = GetNode<TextureRect>("Icon");
    }

    public void Bind(PlayerWater waterSystem)
    {
        _water = waterSystem;
        _water.OnWaterChanged += UpdateUI;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_water == null)
            return;

        float ratio = _water.GetNormalized();

        _bar.Value = ratio * 100f;

        if (ratio > 0.7f)
            _icon.Texture = FullIcon;
        else if (ratio > 0.3f)
            _icon.Texture = MidIcon;
        else
            _icon.Texture = EmptyIcon;
    }
}