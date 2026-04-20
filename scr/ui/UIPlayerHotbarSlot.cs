using Godot;

public partial class UIPlayerHotbarSlot : Control
{
    private TextureRect Background;
    private TextureRect Icon;
    private Label StackLabel;

    public override void _Ready()
    {
        Background = GetNode<TextureRect>("Background");
        Icon = GetNode<TextureRect>("Icon");
        StackLabel = GetNode<Label>("Stack");
    }

    public void SetItem(ItemStack stack)
    {
        if (stack == null)
        {
            Icon.Visible = false;
            StackLabel.Visible = false;
            return;
        }

        Icon.Texture = stack.Data.Icon;
        Icon.Visible = true;

        if (stack.Data.Stackable && stack.Amount > 1)
        {
            StackLabel.Text = stack.Amount.ToString();
            StackLabel.Visible = true;
        }
        else
        {
            StackLabel.Visible = false;
        }
    }

    public void SetSelected(bool selected)
    {
        Modulate = selected ? Colors.White : new Color(0.7f, 0.7f, 0.7f);
    }
}