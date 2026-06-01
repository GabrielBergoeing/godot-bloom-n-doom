using Godot;
using System.Collections.Generic;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }
    private UIService UI => UIService.Instance;

    [Export] private ItemDatabase _itemDatabase;

    public LevelData CurrentLevel { get; private set; }
    public List<LobbyPlayerData> LobbyPlayers { get; private set; } = new();

    private LevelData _testLevel;

    public override void _Ready()
    {
        if (Instance != null && Instance != this)
        {
            QueueFree();
            return;
        }

        Instance = this;
        _testLevel = GD.Load<LevelData>(
            "res://nodes/scenes/levels/data/test_level.tres"
        );

        if (_itemDatabase != null)
        {
            GD.Print($"[GameManager] Registering ItemDB with " + $"{_itemDatabase.Items.Length} items");
            ItemDatabase.Register(_itemDatabase);
        }
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
        SceneManager.Instance.ChangeScene(UI.Paths.LevelManagerScene);
    }

    public void StartMatch(Node levelNode)
    {
        MatchManager match = levelNode.GetNodeOrNull<MatchManager>("MatchManager");
        if (match == null)
        {
            GD.PushError("[GameManager] MatchManager not found.");
            SceneManager.Instance.ChangeScene(UI.Paths.MainMenuScene);
            return;
        }

        UI.PlayBGMTrack(CurrentLevel.BGMTrack);
        match.StartMatch();
    }
}