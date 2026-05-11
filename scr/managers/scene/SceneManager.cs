using Godot;
using System;
using System.Threading.Tasks;

public partial class SceneManager : Node
{
    public static SceneManager Instance { get; private set; }
    public LevelData CurrentLevel { get; private set; }

    private string _levelManagerPath = "res://nodes/scenes/menu/level_manager.tscn";

    public override void _Ready()
    {
        Instance = this;
    }

    public async void ChangeScene(string scenePath)
    {
        GD.Print("[SceneManager] Starting scene transition...");

        // Future fade out
        /*
        if (UIService.Instance?.Fade != null)
        {
            await UIService.Instance.Fade.FadeOut();
        }
        */

        Error result = GetTree().ChangeSceneToFile(scenePath);

        if (result != Error.Ok)
        {
            GD.PushError($"[SceneManager] Failed to load scene: {scenePath}");
            return;
        }

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        GD.Print("[SceneManager] Scene Changed!");

        // Future fade in
        /*
        if (UIService.Instance?.Fade != null)
        {
            await UIService.Instance.Fade.FadeIn();
        }
        */
    }

    public void ChangeLevelScene(LevelData levelData)
    {
        CurrentLevel = levelData;
        ChangeScene(_levelManagerPath);
    }
}