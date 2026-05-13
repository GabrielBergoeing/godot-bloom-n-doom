using Godot;
using System;

public partial class UIMainMenu : Control
{
    private UIService UI => UIService.Instance;

    [ExportGroup("Title Animation Variables")]
    [Export] public float FloatSpeed = 1.5f;
    [Export] public float FloatAmount = 12f;

    private Vector2 _logoStartPos;
    private float _floatTime = 0f;

    private TextureRect _logo;
    private TextureButton _playButton;
    private TextureButton _settingsButton;
    private TextureButton _quitButton;

    public override void _Ready()
    {
        _logo = GetNode<TextureRect>("Logo");
        _playButton = GetNode<TextureButton>("Buttons/PlayButton");
        _settingsButton = GetNode<TextureButton>("Buttons/SettingsButton");
        _quitButton = GetNode<TextureButton>("Buttons/QuitButton");

        _playButton.Pressed += PlayBTN;
        _settingsButton.Pressed += SettingsBTN;
        _quitButton.Pressed += QuitGameBTN;

        _playButton.MouseEntered += HoverBTN;
        _settingsButton.MouseEntered += HoverBTN;
        _quitButton.MouseEntered += HoverBTN;
        
        _logoStartPos = _logo.Position;
        _playButton.GrabFocus();
    }

    public override void _Process(double delta)
    {
        _floatTime += (float)delta;
        float offsetY = Mathf.Sin(_floatTime * FloatSpeed) * FloatAmount;

        _logo.Position = new Vector2(
            _logoStartPos.X,
            _logoStartPos.Y + offsetY
        );
    }

    private void PlayBTN()
    {
        GD.Print("Play button pressed.");
        // UI.Sfx.PlayOnConfirm();
        UI.Scene.ChangeScene(UI.Paths.LobbyMenuScene);
    }

    private void SettingsBTN()
    {
        GD.Print("Settings button pressed.");
        // UI.Sfx.PlayOnToggle();
        // UI.Menu.ShowSettingsOverlay();
    }

    private void QuitGameBTN()
    {
        GD.Print("Quit button pressed.");
        // UI.Sfx.PlayOnToggle();
        GetTree().Quit();
    }

    private void HoverBTN()
    {
        GD.Print("Hover button.");
        // UI.Sfx.PlayOnHover();
    }

    private void ToggleBTN()
    {
        GD.Print("Toggle button.");
        // UI.Sfx.PlayOnToggle();
    }
}