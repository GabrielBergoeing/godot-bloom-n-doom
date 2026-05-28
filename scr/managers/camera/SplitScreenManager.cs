using Godot;
using System.Collections.Generic;

public partial class SplitScreenManager : Node
{
    public static SplitScreenManager Instance;
    public NetworkRoot Network => NetworkRoot.Instance;

    [Export] public PackedScene UIPlayerViewportScene;
    [Export] public PackedScene PlayerScene;
    [Export] public PackedScene MatchResultsScene;

    private GridContainer _screenContainer;
    private Node _levelNode;
    public Node LevelNode => _levelNode;

    private MatchManager _matchManager;
    private UIMatchResults _matchResults;
    private World2D _sharedWorld;

    private readonly List<UIPlayerViewport> _viewports = new();
    private const float BaseHeight = 162f;

    public override void _Ready()
    {
        Instance = this;

        _screenContainer =GetNode<GridContainer>("CenterContainer/GridContainer");
        if (Network.IsOnline)
            PrepareOnlineMatch();
        else
            StartOfflineMatch();
    }

    public override void _ExitTree()
    {
        if (Network.IsOnline)
            SteamMatchManager.Instance.OnMatchStarted -= OnMatchStarted;
    }

    private void StartOfflineMatch()
    {
        CreateLevel();
        SpawnOfflinePlayers();
        FinalizeMatchSetup();
    }

    private void PrepareOnlineMatch()
    {
        SteamMatchManager.Instance.OnMatchStarted += OnMatchStarted;

        if (Network.Match.IsHost)
            SteamMatchManager.Instance.BroadcastMatchStart();
    }

    private void OnMatchStarted(MatchStartPacket packet)
    {
        GD.Print("[SplitScreenManager] MatchStart received");

        CreateLevel();
        SpawnOnlinePlayers(packet);
        FinalizeMatchSetup();
    }

    private void CreateLevel()
    {
        UIPlayerViewport viewport = CreateViewport();

        LevelData levelData = GameManager.Instance.CurrentLevel;
        _levelNode = levelData.LevelScene.Instantiate<Node>();

        viewport.GetSubViewport().AddChild(_levelNode);

        _sharedWorld = viewport.GetSubViewport().World2D;
        _matchManager =_levelNode.GetNode<MatchManager>("MatchManager");
    }

    private void SpawnOfflinePlayers()
    {
        var players = GameManager.Instance.LobbyPlayers;
        for (int i = 0; i < players.Count; i++)
            SpawnOfflinePlayer(players[i], i);
    }

    private void SpawnOnlinePlayers(MatchStartPacket packet)
    {
        foreach (var spawnData in packet.Players)
            SpawnNetworkPlayer(spawnData);
    }

    private void SpawnOfflinePlayer(LobbyPlayerData data, int spawnIndex)
    {
        Player player = CreatePlayer(data);
        player.Position = _matchManager.GetSpawnPosition(
            spawnIndex
        );

        CreateLocalViewport(player);
    }

    private void SpawnNetworkPlayer(PlayerSpawnData data)
    {
        LobbyPlayerData lobbyData = GameManager.Instance.LobbyPlayers
            .Find(p => p.PlayerId == data.PlayerId);

        if (lobbyData == null)
        {
            GD.PrintErr($"Missing LobbyPlayerData for {data.PlayerId}");
            return;
        }

        Player player = CreatePlayer(lobbyData);
        player.Position = _matchManager.GetSpawnPosition(
            data.SpawnIndex
        );

        player.SetNetworkOwnership(
            data.SteamId,
            Network.Lobby.LocalSteamId
        );

        if (!player.IsLocallyControlled) return;
        CreateLocalViewport(player);
    }

    private Player CreatePlayer(LobbyPlayerData data)
    {
        Player player = PlayerScene.Instantiate<Player>();
        LevelNode.AddChild(player);

        player.Setup(
            data.PlayerId,
            data.DeviceId,
            data.DeviceType,
            data.SelectedCharacter.Sprites
        );

        _matchManager.RegisterPlayer(player);
        return player;
    }

    private void CreateLocalViewport(Player player)
    {
        UIPlayerViewport viewport;

        if (_viewports.Count == 1 && _viewports[0].GetLinkedPlayer() == null)
            viewport = _viewports[0];
        else
        {
            viewport = CreateViewport();
            viewport.SetSubPortWorld(_sharedWorld);
        }

        viewport.LinkPlayer(player);
        viewport.LinkPlayerUI(player);

        ApplyCameraBounds(viewport);
    }

    private UIPlayerViewport CreateViewport()
    {
        UIPlayerViewport viewport = UIPlayerViewportScene.Instantiate<UIPlayerViewport>();
        _screenContainer.AddChild(viewport);
        _viewports.Add(viewport);

        return viewport;
    }

    private void ApplyCameraBounds(UIPlayerViewport viewport)
    {
        if (_levelNode == null) return;

        TileMapLayer tilemap =_levelNode.GetNode<TileMapLayer>("World");

        viewport.SetCameraBounds(
            tilemap.GetUsedRect(),
            tilemap.TileSet.TileSize
        );
    }

    private void FinalizeMatchSetup()
    {
        UpdateViewportLayout();
        GameManager.Instance.StartMatch(_levelNode);
        CreateMatchResultsPanel();
    }

    private void UpdateViewportLayout()
    {
        int count = _viewports.Count;
        if (count == 0) return;

        int columns = Mathf.CeilToInt(count / 2.0f);
        _screenContainer.Columns = columns;

        Vector2 size = GetViewport().GetVisibleRect().Size;
        Vector2I viewportSize = CalculateViewportSize(
            count,
            columns,
            size
        );

        foreach (var viewport in _viewports)
        {
            viewport.UpdateViewportSize(
                viewportSize,
                BaseHeight,
                count
            );
        }
    }

    private Vector2I CalculateViewportSize(int count, int columns,Vector2 screenSize)
    {
        int rows =Mathf.CeilToInt(count / (float)columns);

        int width = (int)(screenSize.X / columns);
        int height = (int)(screenSize.Y / rows);

        width -= width % 2;
        height -= height % 2;

        return new Vector2I(width, height);
    }

    private void CreateMatchResultsPanel()
    {
        _matchResults = MatchResultsScene
            .Instantiate<UIMatchResults>();

        AddChild(_matchResults);
    }
}