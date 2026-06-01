using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class MatchManager : Node
{
    public static MatchManager Instance;
    public GameManager Game => GameManager.Instance;
    public NetworkRoot Network => NetworkRoot.Instance;

    [Signal]  public delegate void MatchEndedEventHandler();

    [ExportGroup("Player Setup")]
    [Export] public Array<Node2D> PlayerSpawnPoints = new();
    [Export] public Array<ItemData> StartingItems = new();

    private readonly Array<Player> players = new();
    private readonly List<Vector2> _spawnPositions = new();

    private readonly ScoreTally scoreTally = new();
    private List<ScoreResult> _results = new();
    public List<ScoreResult> Results => _results;

    private bool isPlayingMatch = false;
    private bool hasPrintedResults = false;

    public float MatchDuration;
    public float Timer { get; private set; }

    private const float TimerSyncInterval = 5f;
    private float _timerSyncClock = 0f;

    public bool IsMatchRunning => isPlayingMatch && !hasPrintedResults;

    public override void _Ready()
    {
        Instance = this;

        MatchDuration = Game.CurrentLevel.MatchDuration;
        Timer = MatchDuration;
        CacheSpawnPositions();
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
            return;
        }

        if (Network.IsOnline && Network.Lobby.IsHost)
            SyncOnlineTimer(delta);
    }

    public void SubscribeToTimerSync()
    {
        if (NetworkRoot.Instance.IsOnline && !NetworkRoot.Instance.Lobby.IsHost)
            SteamMatchManager.Instance.OnTimerSyncReceived += OnTimerSyncReceived;
    }

    public void UnsubscribeFromTimerSync()
    {
        if (SteamMatchManager.Instance != null)
            SteamMatchManager.Instance.OnTimerSyncReceived -= OnTimerSyncReceived;
    }

    public void StartMatch()
    {
        GiveStartingItems();

        isPlayingMatch = true;
        hasPrintedResults = false;

        Callable.From(SpawnRegisteredPlayers).CallDeferred();
    }

    public void EndMatch()
    {
        hasPrintedResults = true;
        DisablePlayerInput();

        _results = GetResults();
        EmitSignal(SignalName.MatchEnded);
    }

    private void SyncOnlineTimer(double delta)
    {
        _timerSyncClock -= (float)delta;
        if (_timerSyncClock <= 0f)
        {
            _timerSyncClock = TimerSyncInterval;
            SteamMatchManager.Instance.BroadcastTimerSync(Timer);
        }
    }

    private void CacheSpawnPositions()
    {
        _spawnPositions.Clear();
        foreach (var point in PlayerSpawnPoints)
        {
            if (point != null)
                _spawnPositions.Add(point.Position);
        }
    }

    public Vector2 GetSpawnPosition(int index)
    {
        if (index < _spawnPositions.Count)
            return _spawnPositions[index];

        Vector2 fallback = new Vector2(index * 32, 0);
        GD.PrintErr($"[MatchManager] GetSpawnPosition({index}) fallback -> {fallback}");
        return fallback;
    }
    
    private void SpawnRegisteredPlayers()
    {
        foreach (Player player in players)
        {
            Vector2 pos = GetSpawnPosition(player.SpawnIndex);
            player.Position = pos;
        }
    }

    public void RegisterPlayer(Player player, int spawnIndex)
    {
        if (players.Contains(player)) return;
        player.SpawnIndex = spawnIndex;
        players.Add(player);
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

    private void OnTimerSyncReceived(float timeRemaining)
    {
        if (Mathf.Abs(Timer - timeRemaining) > 1f)
            Timer = timeRemaining;
    }

    public Array<Player> GetPlayers() => players;
}