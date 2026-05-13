using Godot;
using System;

public partial class UIService : Node
{
    public static UIService Instance { get; private set; }

    public GameManager Game => GameManager.Instance;
    public SceneManager Scene => SceneManager.Instance;
    public AudioManager Audio => AudioManager.Instance;

    // Created on the spot
    public GamePaths Paths { get; private set;} = new();
    // public UIFadeScreen Fade { get; private set; } = new();
    public UISFX SFX { get; private set; }
    private AudioStreamPlayer _uiPlayer;

    public override void _Ready()
    {
        Instance = this;

        _uiPlayer = new AudioStreamPlayer();
        _uiPlayer.Bus = "UI";

        AddChild(_uiPlayer);

        SFX = new UISFX(
            _uiPlayer,
            Paths.ConfirmAudio,
            Paths.ToggleAudio,
            Paths.HoverAudio
        );
    }
}