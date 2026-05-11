using Godot;

public partial class UIPlayerHotbarSlot : Control
{
    private TextureRect _background;
    private TextureRect _icon;
    private Label _stackLabel;

    public override void _Ready()
    {
        _background = GetNode<TextureRect>("Background");
        _icon = GetNode<TextureRect>("Icon");
        _stackLabel = GetNode<Label>("Stack");
    }

    public void SetItem(ItemStack stack)
    {
        if (stack == null)
        {
            _icon.Visible = false;
            _stackLabel.Visible = false;
            return;
        }

        _icon.Texture = stack.Data.Icon;
        _icon.Visible = true;

        if (stack.Data.Stackable && stack.Amount > 1)
        {
            _stackLabel.Text = stack.Amount.ToString();
            _stackLabel.Visible = true;
        }
        else
            _stackLabel.Visible = false;
    }

    public void SetSelected(bool selected)
    {
        Modulate = selected ? Colors.White : new Color(0.7f, 0.7f, 0.7f);
    }
}