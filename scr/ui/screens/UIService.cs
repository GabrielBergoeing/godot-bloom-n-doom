using Godot;
using System;

public partial class UIService : Node
{
    public static UIService Instance { get; private set; }

    // Future systems
    // public UISFX Sfx { get; private set; }
    // public UIFadeScreen Fade { get; private set; }
    // public MenuManager Menu { get; private set; }

    public override void _Ready()
    {
        Instance = this;

        GD.Print("[UIService] Initialized.");

        // Future setup
        /*
        Sfx = GetNode<UISFX>("UISFX");
        Fade = GetNode<UIFadeScreen>("UIFadeScreen");

        if (Sfx == null)
            GD.PushError("[UIService] Missing UISFX!");

        if (Fade == null)
            GD.PushWarning("[UIService] Missing UIFadeScreen!");
        */

        // Optional future lookup
        /*
        Menu = GetTree().Root.GetNode<MenuManager>("PathToMenuManager");

        if (Menu != null)
            GD.Print("[UIService] MenuManager found.");
        */
    }
}