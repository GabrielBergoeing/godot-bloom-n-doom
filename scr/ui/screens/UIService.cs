using Godot;
using System;

public partial class UIService : Node
{
    public static UIService Instance { get; private set; }

    // Future systems
    // public UISFX Sfx { get; private set; }
    // public UIFadeScreen Fade { get; private set; }
    public SceneManager Scene { get; private set; }

    // Scenes
    public string MainMenu { get; private set; } = "res://nodes/scenes/menu/main_menu.tscn";
    public string LobbyMenu { get; private set; } = "res://nodes/scenes/menu/lobby_menu.tscn";
    public LevelData TestLevel { get; private set; }

    private string _testLevelData = "res://nodes/scenes/levels/data/test_level.tres";

    public override void _Ready()
    {
        Instance = this;
        Scene = SceneManager.Instance;
        
        TestLevel = GD.Load<LevelData>(_testLevelData);

        // Future setup
        /*
        Sfx = GetNode<UISFX>("UISFX");
        Fade = GetNode<UIFadeScreen>("UIFadeScreen");

        if (Sfx == null)
            GD.PushError("[UIService] Missing UISFX!");

        if (Fade == null)
            GD.PushWarning("[UIService] Missing UIFadeScreen!");
        */
    }
}