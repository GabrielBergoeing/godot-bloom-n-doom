using Godot;

public partial class UIPlayerHotbar : Control
{
    [Export] public PackedScene UIPlayerHotbarSlotScene { get; set; }
    private PlayerHotbar Hotbar;
    private UIPlayerHotbarSlot[] Slots;

    public void Bind(PlayerHotbar hotbar)
    {
        this.Hotbar = hotbar;

        SetSlots();
        Hotbar.OnSlotChanged += UpdateUI;
        UpdateUI();
        AnchorToViewport();
    }

    private void SetSlots()
    {
        int size = Hotbar.SlotCount;
        Slots = new UIPlayerHotbarSlot[size];
        
        var containter = GetNode<HBoxContainer>($"HBoxContainer");
        for (int i = 0; i < size; i++)
        {
            var slot = UIPlayerHotbarSlotScene.Instantiate<UIPlayerHotbarSlot>();
            slot.Name = $"Slot{i}";
            Slots[i] = slot;
            containter.AddChild(slot);
        }
    }

    private void AnchorToViewport()
    {
        var viewport = GetViewport();
        Size = viewport.GetVisibleRect().Size;

        AnchorLeft   = 0f;
        AnchorRight  = 1f;
        AnchorTop    = 1f;
        AnchorBottom = 1f;

        OffsetTop    = -200;
        //OffsetRight = 200;
    }

    private void UpdateUI()
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            var stack = Hotbar.GetStackAt(i);

            Slots[i].SetItem(stack);
            Slots[i].SetSelected(i == Hotbar.CurrentSlot);
        }
    }
}