using Godot;
using System;
using System.Threading.Tasks;

public partial class SceneManager : Node
{
    public static SceneManager Instance { get; private set; }
    public event Action OnSceneReady;

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
        SendSceneChangeSignal();

        if (Fade != null)
            await Fade.FadeIn();
    }

    private async void SendSceneChangeSignal()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        GD.Print("[SceneManager] Scene ready");
        OnSceneReady?.Invoke();
    }

    private async void ReadyFadeScreen()
    {
        await Fade.FadeIn();
    }
}