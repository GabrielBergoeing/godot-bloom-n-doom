using Godot;
using System;

public partial class UIService : Node
{
    public static UIService Instance { get; private set; }

    // Future systems
    // public UISFX Sfx { get; private set; }
    // public UIFadeScreen Fade { get; private set; }
    public SceneManager Scene { get; private set; }
    public GameManager Game { get; private set; }
    public GamePaths Paths { get; private set;} = new();

    public override void _Ready()
    {
        Instance = this;
        Game = GameManager.Instance;
        Scene = SceneManager.Instance;

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