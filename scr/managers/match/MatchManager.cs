using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class MatchManager : Node
{
    public static MatchManager Instance;
    public GameManager Game => GameManager.Instance;

    [ExportGroup("Player Setup")]
    [Export] public Array<Node2D> PlayerSpawnPoints = new();
    [Export] public Array<ItemData> StartingItems = new();

    private readonly Array<Player> players = new();
    private readonly ScoreTally scoreTally = new();

    private bool isPlayingMatch = false;
    private bool hasPrintedResults = false;

    public float MatchDuration;
    public float Timer { get; private set; }
    public bool IsMatchRunning => isPlayingMatch && !hasPrintedResults;

    public override void _Ready()
    {
        Instance = this;
        MatchDuration = Game.CurrentLevel.MatchDuration;
        Timer = MatchDuration;
    }

    public override void _Process(double delta)
    {
        //HandlePauseInput();

        if (!isPlayingMatch)
            return;

        Timer -= (float)delta;
        if (Timer <= 0f && !hasPrintedResults)
        {
            Timer = 0f;
            EndMatch();
        }
    }

    public void StartMatch()
    {
        InitializePlayers();
        SpawnPlayers();
        GiveStartingItems();

        isPlayingMatch = true;
        hasPrintedResults = false;

        GD.Print("[MatchManager] Match Started");
    }

    public void EndMatch()
    {
        //PauseMatch(true);
        hasPrintedResults = true;

        GD.Print("[MatchManager] Match Ended");
        PrintResults();
    }

    public void InitializePlayers()
    {
        players.Clear();

        foreach (Node child in GetTree().GetNodesInGroup("players"))
        {
            if (child is Player player)
                players.Add(player);
        }

        GD.Print($"[MatchManager] Registered {players.Count} players");
    }

    private void SpawnPlayers()
    {
        if (players.Count == 0)
        {
            GD.PrintErr(
                "[MatchManager] No players found"
            );

            return;
        }

        for (int i = 0; i < players.Count; i++)
        {
            Player player = players[i];
            Vector2 spawnPos = GetSpawnPosition(i);

            player.Position = spawnPos;
            GD.Print($"[MatchManager] Spawned Player {player.PlayerId} at {spawnPos}");
        }
    }

    private Vector2 GetSpawnPosition(int index)
    {
        if (index < PlayerSpawnPoints.Count && PlayerSpawnPoints[index] != null)
            return PlayerSpawnPoints[index].GlobalPosition;

        return new Vector2(index * 32, 0);
    }

    private void GiveStartingItems()
    {
        foreach (Player player in players)
        {
            var hotbar = player.Hotbar;

            if (hotbar == null)
                continue;

            foreach (ItemData item in StartingItems)
            {
                if (item == null)
                    continue;

                hotbar.AddItem(item);
            }
        }
    }

    private void PrintResults()
    {
        GD.Print("========== MATCH RESULTS ==========");

        List<Player> playerList = players.ToList();
        List<ScoreResult> results = scoreTally.DeterminePlacements(
                playerList,
                FarmManager.Instance);

        if (results.Count == 0)
            GD.Print("No scores registered.");

        for (int i = 0; i < results.Count; i++)
        {
            ScoreResult result = results[i];

            GD.Print(
                $"#{i + 1} " +
                $"{result.PlayerName} " +
                $"- {result.Score} pts"
            );
        }

        GD.Print("===================================");
    }

    public Array<Player> GetPlayers() => players;
}
//TODO: PAUSE LOGIC
    /*
    public void TogglePause()
    {
        PauseMatch(!GetTree().Paused);
    }

    public void PauseMatch(bool pause)
    {
        GetTree().Paused = pause;

        isPlayingMatch = !pause;

        foreach (Player player in players)
        {
            // Future:
            // Disable player controls
            // Swap action maps
            // Open pause UI
        }

        GD.Print(
            pause
                ? "[MatchManager] Match Paused"
                : "[MatchManager] Match Resumed"
        );
    }

    public void PauseMatch() =>
        PauseMatch(true);

    public void UnpauseMatch() =>
        PauseMatch(false);

    private void HandlePauseInput()
    {
        if (!IsMatchRunning)
            return;

        foreach (Player player in players)
        {
            // Future:
            // Dedicated player pause input

            if (Input.IsActionJustPressed("pause"))
            {
                TogglePause();
                return;
            }
        }
    }
    */