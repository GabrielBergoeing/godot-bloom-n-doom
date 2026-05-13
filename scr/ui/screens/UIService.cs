using Godot;
using System;

public partial class UIService : Node
{
    public static UIService Instance { get; private set; }

    public GameManager Game => GameManager.Instance;
    public SceneManager Scene => SceneManager.Instance;
    public AudioManager Audio => AudioManager.Instance;

    public GamePaths Paths { get; private set;} = new();
    public UISFX SFX { get; private set; }

    [Export] private AudioStreamPlayer _uiPlayer;

    public override void _Ready()
    {
        Instance = this;
        _uiPlayer.Bus = "UI";

        SFX = new UISFX(
            _uiPlayer,
            Paths.ConfirmAudio,
            Paths.ToggleAudio,
            Paths.HoverAudio
        );
    }
}