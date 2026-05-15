using Godot;

public partial class UILevelSelectMenu : Control
{
    private UIService UI => UIService.Instance;

    [Export] public Godot.Collections.Array<LevelData> Levels = new();
    [Export] private HBoxContainer _buttonsContainer;

    private TextureButton _firstButton;

    public override void _Ready()
    {
        if(_buttonsContainer == null)
            _buttonsContainer = GetNode<HBoxContainer>("Levels");
        RegisterButtons();
    }

    private void RegisterButtons()
    {
        for (int i = 0; i < Levels.Count; i++)
        {
            int index = i;
            if (_buttonsContainer.GetChild(i) is not TextureButton button)
                continue;

            ConfigureButton(button, index);
        }
    }

    private void ConfigureButton(TextureButton button, int index)
    {
        //button.Text = Levels[i].LevelName;
        button.Pressed += () => SelectLevel(index);
        button.FocusEntered += HoverBTN;

        var label = button.GetNode<Label>("Label");
        label.Text = $"{index + 1}";

        if(_firstButton == null)
        {
            _firstButton = button;
            _firstButton.GrabFocus();
        }
    }

    private void SelectLevel(int index)
    {
        if (index < 0 || index >= Levels.Count)
            return;

        LevelData level = Levels[index];

        UI.SFX.PlayOnConfirm();
        UI.Game.LoadLevel(level);
    }

    private void HoverBTN()
    {
        UI.SFX.PlayOnHover();
    }
}