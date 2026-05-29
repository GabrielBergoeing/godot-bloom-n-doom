using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class MatchManager : Node
{
    public static MatchManager Instance;
    public GameManager Game => GameManager.Instance;

    [Signal]  public delegate void MatchEndedEventHandler();

    [ExportGroup("Player Setup")]
    [Export] public Array<Node2D> PlayerSpawnPoints = new();
    [Export] public Array<ItemData> StartingItems = new();

    private readonly Array<Player> players = new();

    private readonly ScoreTally scoreTally = new();
    private List<ScoreResult> _results = new();
    public List<ScoreResult> Results => _results;

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
        GiveStartingItems();

        isPlayingMatch = true;
        hasPrintedResults = false;

        GD.Print("[MatchManager] Match Started");
        Callable.From(SpawnRegisteredPlayers).CallDeferred();
    }

    public void EndMatch()
    {
        hasPrintedResults = true;
        DisablePlayerInput();

        _results = GetResults();
        EmitSignal(SignalName.MatchEnded);
    }

    public void RegisterPlayer(Player player)
    {
        if (players.Contains(player))
            return;

        players.Add(player);
        GD.Print($"[MatchManager] Registered Player {player.PlayerId}");
    }

    public Vector2 GetSpawnPosition(int index)
    {
        if (index < PlayerSpawnPoints.Count && PlayerSpawnPoints[index] != null)
            return PlayerSpawnPoints[index].GlobalPosition;

        return new Vector2(index * 32, 0);
    }


    private void SpawnRegisteredPlayers()
    {
        foreach (Player player in players)
        {
            Vector2 pos = GetSpawnPosition(player.SpawnIndex);
            player.GlobalPosition = pos;
            GD.Print($"[MatchManager] Spawned Player {player.PlayerId} at {pos}");
        }
    }

    public void RegisterPlayer(Player player, int spawnIndex)
    {
        if (players.Contains(player)) return;
        player.SpawnIndex = spawnIndex;
        players.Add(player);
        GD.Print($"[MatchManager] Registered Player {player.PlayerId} at spawn {spawnIndex}");
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

    private List<ScoreResult> GetResults()
    {
        List<Player> playerList = players.ToList();

        return scoreTally.DeterminePlacements(playerList, FarmManager.Instance);
    }

    private void DisablePlayerInput()
    {
        foreach (Player player in players)
            player.Input.SetMatchStatus(false);
    }

    public Array<Player> GetPlayers() => players;
}