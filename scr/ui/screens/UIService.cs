using Godot;
using System;

public partial class UIService : Node
{
    public static UIService Instance { get; private set; }

    public GameManager Game => GameManager.Instance;
    public SceneManager Scene => SceneManager.Instance;
    public AudioManager Audio => AudioManager.Instance;

    public GamePaths Paths { get; private set;} = new();
    public UIFadeScreen Fade { get; private set; }
    public UISFX SFX { get; private set; }

    private AudioStreamPlayer _uiPlayer = new();

    public override void _Ready()
    {
        Instance = this;

        _uiPlayer.Bus = "UI";
        AddChild(_uiPlayer);

        SFX = new UISFX(
            _uiPlayer,
            Paths.ConfirmAudio,
            Paths.ToggleAudio,
            Paths.HoverAudio
        );

        ReadyFadeScreen();
    }

    private async void ReadyFadeScreen()
    {
        Fade = GetNode<UIFadeScreen>("UIFadeScreen");
        await Fade.FadeIn();
    }
}