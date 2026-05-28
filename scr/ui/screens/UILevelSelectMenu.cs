using Godot;

public partial class UILevelSelectMenu : Control
{
    private UIService UI => UIService.Instance;

    [Export] public Godot.Collections.Array<LevelData> Levels = new();
    [Export] private HBoxContainer _buttonsContainer;
    [Export] private Label _descriptionContainer;
    [Export] private Label _levelName;
    [Export] private Label _waitingLabel;

    private TextureButton _firstButton;

    public override void _Ready()
    {
        if(_buttonsContainer == null)
            _buttonsContainer = GetNode<HBoxContainer>("Levels");

        if(_descriptionContainer == null)
            _descriptionContainer = GetNode<Label>("LevelDescription/Label");

        if(_levelName == null)
            _levelName = GetNode<Label>("LevelName/LevelLabel");
        
        if(_waitingLabel == null)
            _waitingLabel = GetNode<Label>("OnlineText/OnlineLabel");

        RegisterButtons();
        SetupOnlineState();
    }

    public override void _ExitTree()
    {
        if (UI.Network.IsOnline)
            UI.Network.Lobby.OnStartGame -= OnStartGameReceived;
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

    private void SetupOnlineState()
    {
        if (!UI.Network.IsOnline)
        {
            if (_waitingLabel != null)
                _waitingLabel.Visible = false;
            return;
        }

        if (UI.Network.Lobby.IsHost)
        {
            // Host: enable buttons, hide waiting label
            if (_waitingLabel != null)
                _waitingLabel.Visible = false;

            SetButtonsDisabled(false);
        }
        else
        {
            // Guest: disable buttons, show waiting label
            if (_waitingLabel != null)
            {
                _waitingLabel.Visible = true;
                _waitingLabel.Text = "Waiting for host to select level...";
            }

            SetButtonsDisabled(true);
            UI.Network.Lobby.OnStartGame += OnStartGameReceived;
        }
    }

    private void SetButtonsDisabled(bool disabled)
    {
        foreach (var child in _buttonsContainer.GetChildren())
        {
            if (child is TextureButton btn)
                btn.Disabled = disabled;
        }
    }


    private void ConfigureButton(TextureButton button, int index)
    {
        button.Pressed += () => SelectLevel(index);
        button.FocusEntered += () => HoverBTN(index);

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

    private void OnStartGameReceived(int levelIndex)
    {
        GD.Print($"[UILevelSelectMenu] Host selected level {levelIndex}");

        if (levelIndex < 0 || levelIndex >= Levels.Count)
        {
            GD.PrintErr($"[UILevelSelectMenu] Invalid level index: {levelIndex}");
            return;
        }

        LevelData level = Levels[levelIndex];
        UI.Game.LoadLevel(level);
    }

    private void HoverBTN(int index)
    {
        UI.SFX.PlayOnHover();
        _descriptionContainer.Text = Levels[index].Description;
        _levelName.Text = Levels[index].LevelName;
    }
}