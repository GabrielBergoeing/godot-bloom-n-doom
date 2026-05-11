using Godot;

public partial class UIPlayerHotbar : Control
{
    [Export] public PackedScene UIPlayerHotbarSlotScene { get; set; }
    private PlayerHotbar _hotbar;
    private UIPlayerHotbarSlot[] _slots;

    public void Bind(PlayerHotbar hotbar)
    {
        this._hotbar = hotbar;

        SetSlots();
        _hotbar.OnSlotChanged += UpdateUI;
        UpdateUI();
        AnchorToViewport();
    }

    private void SetSlots()
    {
        int size = _hotbar.SlotCount;
        _slots = new UIPlayerHotbarSlot[size];
        
        var containter = GetNode<HBoxContainer>($"HBoxContainer");
        for (int i = 0; i < size; i++)
        {
            var slot = UIPlayerHotbarSlotScene.Instantiate<UIPlayerHotbarSlot>();
            slot.Name = $"Slot{i}";
            _slots[i] = slot;
            containter.AddChild(slot);
        }
    }
    
    private void AnchorToViewport()
    {
        AnchorLeft   = 0.5f;
        AnchorRight  = 0.5f;
        AnchorTop    = 1f;
        AnchorBottom = 1f;

        OffsetLeft   = -212.5f;
        OffsetRight  = 212.5f;
        OffsetTop    = -110;
        OffsetBottom = 0;
    }

    private void UpdateUI()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            var stack = _hotbar.GetStackAt(i);

            _slots[i].SetItem(stack);
            _slots[i].SetSelected(i == _hotbar.CurrentSlot);
        }
    }
}