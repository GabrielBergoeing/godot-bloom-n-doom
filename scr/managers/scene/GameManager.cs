using Godot;
using System.Collections.Generic;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    public LevelData CurrentLevel { get; private set; }
    public List<LobbyPlayerData> LobbyPlayers = new();

    private LevelData _testLevel;

    public override void _Ready()
    {
        if (Instance != null && Instance != this)
        {
            QueueFree();
            return;
        }

        Instance = this;
        _testLevel = GD.Load<LevelData>("res://nodes/scenes/levels/data/test_level.tres");
        ProcessMode = ProcessModeEnum.Always;
    }

    public void SetLobbyPlayers(IEnumerable<LobbyPlayerData> players)
    {
        LobbyPlayers.Clear();
        LobbyPlayers.AddRange(players);
    }

    public void ClearLobby()
    {
        LobbyPlayers.Clear();
    }

    public void SetLevel(string levelDataPath)
    {
        CurrentLevel = GD.Load<LevelData>(levelDataPath) ?? _testLevel;
    }

    public void LoadLevel(string levelDataPath)
    {   
        SetLevel(levelDataPath);

        string levelManagerScene = UIService.Instance.Paths.LevelManagerScene;
        SceneManager.Instance.ChangeScene(levelManagerScene);
    }

    public void StartMatch(Node levelNode)
    {
        MatchManager match = levelNode.GetNodeOrNull<MatchManager>("MatchManager");
        if (match == null)
        {
            GD.PushError("[GameManager] MatchManager not found in level.");
            string mainMenuScene = UIService.Instance.Paths.MainMenuScene;
            SceneManager.Instance.ChangeScene(mainMenuScene);
        }

        match.StartMatch();
    }
}