using Godot;
using System;
using System.Threading.Tasks;

public partial class SceneManager : Node
{
    public static SceneManager Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public async void ChangeScene(string scenePath)
    {
        GD.Print("[SceneManager] Starting scene transition...");

        if (UIService.Instance?.Fade != null)
            await UIService.Instance.Fade.FadeOut();

        Error result = GetTree().ChangeSceneToFile(scenePath);
        if (result != Error.Ok)
        {
            GD.PushError($"[SceneManager] Failed to load scene: {scenePath}");
            return;
        }

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        if (UIService.Instance?.Fade != null)
            await UIService.Instance.Fade.FadeIn();
    }
}