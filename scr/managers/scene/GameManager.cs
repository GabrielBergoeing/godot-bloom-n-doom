using Godot;
using System.Collections.Generic;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    private UIService UI => UIService.Instance;

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

    public void SetLevel(LevelData levelData)
    {
        CurrentLevel = levelData ?? _testLevel;
    }

    public void LoadLevel(LevelData levelData)
    {
        SetLevel(levelData);

        string levelManagerScene = UI.Paths.LevelManagerScene;
        SceneManager.Instance.ChangeScene(levelManagerScene);
    }

    public void StartMatch(Node levelNode)
    {
        MatchManager match = levelNode.GetNodeOrNull<MatchManager>("MatchManager");
        if (match == null)
        {
            GD.PushError("[GameManager] MatchManager not found in level.");
            string mainMenuScene = UI.Paths.MainMenuScene;
            SceneManager.Instance.ChangeScene(mainMenuScene);
        }

        AudioManager.Instance.StartBGM(CurrentLevel.BGMTrack);
        match.StartMatch();
    }
}