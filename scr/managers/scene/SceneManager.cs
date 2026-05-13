using Godot;
using System;
using System.Threading.Tasks;

public partial class SceneManager : Node
{
    public static SceneManager Instance { get; private set; }
    [Export] private UIFadeScreen Fade;

    public override void _Ready()
    {
        Instance = this;
        ReadyFadeScreen();
    }

    public async void ChangeScene(string scenePath)
    {
        GD.Print("[SceneManager] Starting scene transition...");

        if (Fade != null)
            await Fade.FadeOut();

        Error result = GetTree().ChangeSceneToFile(scenePath);
        if (result != Error.Ok)
        {
            GD.PushError($"[SceneManager] Failed to load scene: {scenePath}");
            return;
        }

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        if (Fade != null)
            await Fade.FadeIn();
    }

    private async void ReadyFadeScreen()
    {
        await Fade.FadeIn();
    }
}