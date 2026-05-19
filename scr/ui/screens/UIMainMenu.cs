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
    private TextureButton _onlineButton;
    private TextureButton _settingsButton;
    private TextureButton _quitButton;

    public override void _Ready()
    {
        _logo = GetNode<TextureRect>("Logo");
        _playButton = GetNode<TextureButton>("Buttons/PlayButton");
        _onlineButton = GetNode<TextureButton>("Buttons/OnlineButton");
        _settingsButton = GetNode<TextureButton>("Buttons/SettingsButton");
        _quitButton = GetNode<TextureButton>("Buttons/QuitButton");

        HookButtonSignals();
        UI.Audio.StartBGM(UI.Paths.MainMenuBGM);
        
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

    private void HookButtonSignals()
    {
        _playButton.Pressed += PlayBTN;
        _onlineButton.Pressed += OnlineBTN;
        _settingsButton.Pressed += SettingsBTN;
        _quitButton.Pressed += QuitGameBTN;

        _playButton.FocusEntered += HoverBTN;
        _onlineButton.FocusEntered += HoverBTN;
        _settingsButton.FocusEntered += HoverBTN;
        _quitButton.FocusEntered += HoverBTN;
    }

    private void PlayBTN()
    {
        UI.SFX.PlayOnConfirm();
        UI.Scene.ChangeScene(UI.Paths.LobbyMenuScene);
    }

    private void OnlineBTN()
    {
        UI.SFX.PlayOnConfirm();
        SteamNetworkManager.Instance.SendPingToSelf();
    }

    private void SettingsBTN()
    {
        UI.SFX.PlayOnConfirm();
        // UI.Menu.ShowSettingsOverlay();
    }

    private void QuitGameBTN()
    {
        UI.SFX.PlayOnConfirm();
        GetTree().Quit();
    }

    private void HoverBTN()
    {
        UI.SFX.PlayOnHover();
    }
}