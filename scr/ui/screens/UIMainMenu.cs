using Godot;
using System;

public partial class UIMainMenu : Control
{
    private UIService UI => UIService.Instance;

    private TextureButton _playButton;
    private TextureButton _settingsButton;
    private TextureButton _quitButton;

    public override void _Ready()
    {
        _playButton = GetNode<TextureButton>("VBoxContainer/PlayButton");
        _settingsButton = GetNode<TextureButton>("VBoxContainer/SettingsButton");
        _quitButton = GetNode<TextureButton>("VBoxContainer/QuitButton");

        _playButton.Pressed += PlayBTN;
        _settingsButton.Pressed += SettingsBTN;
        _quitButton.Pressed += QuitGameBTN;

        _playButton.MouseEntered += HoverBTN;
        _settingsButton.MouseEntered += HoverBTN;
        _quitButton.MouseEntered += HoverBTN;

        _playButton.GrabFocus();
    }

    private void PlayBTN()
    {
        GD.Print("Play button pressed.");
        // UI.Sfx.PlayOnConfirm();
        // GameManager.Instance.ChangeScene("MatchMenu");
        // GetTree().ChangeSceneToFile("res://Scenes/MatchMenu.tscn");
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