using Godot;
using System.Collections.Generic;

public partial class StressTestSetup : Node
{
    [ExportGroup("Options")]
    [Export] public bool StaggerPlacement = false;
    [Export] public float StaggerRowDelay = 0.05f;

    private MatchManager _match;
    private FarmManager _farm;
    private FarmNetworkService _farmService;
    private NetworkRoot _network;

    private bool _hasRun = false;
    private Godot.Collections.Array<SeedData> _seeds = new();

    public override void _Ready()
    {
        _match = MatchManager.Instance;
        _farm = FarmManager.Instance;
        _network = NetworkRoot.Instance;

        if (_match == null)
        {
            GD.PrintErr("[StressTestSetup] MatchManager not found as sibling. Aborting.");
            return;
        }

        if (_farm == null)
        {
            GD.PrintErr("[StressTestSetup] FarmManager.Instance is null. Aborting.");
            return;
        }

        _seeds = GameManager.Instance.CurrentLevel?.GetSeedTableItems();
        if (_seeds == null || _seeds.Count == 0)
        {
            GD.PrintErr("[StressTestSetup] No seeds exported in levelData. Assign at least one SeedData. Aborting.");
            return;
        }

        _match.MatchEnded += OnMatchEnded;
        _match.Connect(MatchManager.SignalName.MatchEnded, Callable.From(OnMatchEnded));
        SetProcess(true);

        GD.Print("[StressTestSetup] Ready — waiting for match to start.");
    }

    public override void _Process(double delta)
    {
        if (_hasRun) return;
        if (_match == null || !_match.IsMatchRunning) return;

        if (_network.IsOnline && !_network.Lobby.IsHost)
        {
            GD.Print("[StressTestSetup] Peer client — skipping local population.");
            _hasRun = true;
            SetProcess(false);
            return;
        }

        _hasRun = true;
        SetProcess(false);

        _farmService = FarmNetworkService.Instance;

        if (StaggerPlacement)
            PopulateStaggered();
        else
            PopulateImmediate();
    }

    public override void _ExitTree()
    {
        if (_match != null)
            _match.MatchEnded -= OnMatchEnded;
    }

    private void PopulateImmediate()
    {
        GD.Print("[StressTestSetup] Populating farm grid immediately...");

        var cells = CollectPreparedCells();
        var players = GetRegisteredPlayerIndices();

        if (players.Count == 0)
        {
            GD.PrintErr("[StressTestSetup] No registered players found. Cannot assign plant ownership.");
            return;
        }

        int seedIdx = 0;
        int playerIdx = 0;
        int planted = 0;

        foreach (var cell in cells)
        {
            if (!_farm.CanPlantSeed(cell))
                continue;

            SeedData seed = _seeds[seedIdx % _seeds.Count];
            int owner = players[playerIdx % players.Count];

            PlantAt(cell, owner, seed);

            seedIdx++;
            playerIdx++;
            planted++;
        }

        GD.Print($"[TestScenarioRunner] Done — {planted} plants placed across {cells.Count} prepared cells.");
    }

    private async void PopulateStaggered()
    {
        GD.Print("[TestScenarioRunner] Populating farm grid with stagger...");

        var cells = CollectPreparedCells();
        var players = GetRegisteredPlayerIndices();

        if (players.Count == 0)
        {
            GD.PrintErr("[TestScenarioRunner] No registered players found. Cannot assign plant ownership.");
            return;
        }

        var rows = new Dictionary<int, List<Vector2I>>();
        foreach (var cell in cells)
        {
            if (!rows.ContainsKey(cell.Y))
                rows[cell.Y] = new List<Vector2I>();
            rows[cell.Y].Add(cell);
        }

        var sortedYs = new List<int>(rows.Keys);
        sortedYs.Sort();

        int seedIdx = 0;
        int playerIdx = 0;
        int planted = 0;

        foreach (int y in sortedYs)
        {
            await ToSignal(GetTree().CreateTimer(StaggerRowDelay), SceneTreeTimer.SignalName.Timeout);

            // Match may have ended during stagger
            if (!IsInstanceValid(this) || !_match.IsMatchRunning)
                break;

            foreach (var cell in rows[y])
            {
                if (!_farm.CanPlantSeed(cell))
                    continue;

                SeedData seed = _seeds[seedIdx % _seeds.Count];
                int owner = players[playerIdx % players.Count];

                PlantAt(cell, owner, seed);

                seedIdx++;
                playerIdx++;
                planted++;
            }
        }

        GD.Print($"[TestScenarioRunner] Done (staggered) — {planted} plants placed.");
    }

    private List<Vector2I> CollectPreparedCells()
    {
        var all = _farm.GetUsedCells();
        var prepared = new List<Vector2I>();

        foreach (var cell in all)
        {
            if (_farm.IsPrepared(cell))
                prepared.Add(cell);
        }

        GD.Print($"[TestScenarioRunner] Found {prepared.Count} prepared cells.");
        return prepared;
    }

    private List<int> GetRegisteredPlayerIndices()
    {
        var result = new List<int>();
        var players = _match.GetPlayers();

        foreach (var player in players)
            result.Add(player.SpawnIndex);

        return result;
    }

    private void PlantAt(Vector2I cell, int playerIndex, SeedData seed)
    {
        if (_network.IsOnline && _farmService != null)
            _farmService.RequestPlantSeed(cell, playerIndex, seed);
        else
            _farm.TryPlantSeed(cell, playerIndex, seed);
    }

    private void OnMatchEnded()
    {
        _hasRun = true;
        SetProcess(false);
    }
}