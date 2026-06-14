using Godot;
using System.Collections.Generic;

public partial class SplitScreenManager : Node
{
    public static SplitScreenManager Instance;
    public NetworkRoot Network => NetworkRoot.Instance;

    [Export] public PackedScene UIPlayerViewportScene;
    [Export] public PackedScene PlayerScene;
    [Export] public PackedScene MatchResultsScene;
    [Export] public CharacterDatabase CharacterDatabase;

    private GridContainer _screenContainer;
    private Node _levelNode;
    public Node LevelNode => _levelNode;

    private PickupNetworkService _pickupService;
    private FarmNetworkService _farmService;
    private ToolNetworkService _toolService;

    private MatchManager _matchManager;
    private UIMatchResults _matchResults;
    private World2D _sharedWorld;

    private readonly List<UIPlayerViewport> _viewports = new();
    private const float BaseHeight = 162f;

    public override void _Ready()
    {
        Instance = this;

        _screenContainer = GetNode<GridContainer>("CenterContainer/GridContainer");
        if (Network.IsOnline)
            PrepareOnlineMatch();
        else
            StartOfflineMatch();
    }

    public override void _ExitTree()
    {
        if (Network.IsOnline)
        {
            SteamMatchManager.Instance.OnMatchStarted -= OnMatchStarted;
            SteamMatchManager.Instance.ResetMatchScene();
            _matchManager?.UnsubscribeFromTimerSync();
        }
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
        SteamMatchManager.Instance.NotifyMatchSceneReady();

        if (Network.Match.IsHost)
            SteamMatchManager.Instance.BroadcastMatchStart();
    }

    private void OnMatchStarted(MatchStartPacket packet)
    {
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

        if (Network.IsOnline)
            CreateNetworkServices();
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
        CharacterData character = data.SelectedCharacter
            ?? CharacterDatabase.Characters[0];

        Player player = PlayerScene.Instantiate<Player>();
        LevelNode.AddChild(player);

        player.Setup(data.PlayerId, character, data.DeviceId, data.DeviceType);
        _matchManager.RegisterPlayer(player, spawnIndex);
        CreateLocalViewport(player);
    }

    private void SpawnNetworkPlayer(PlayerSpawnData data)
    {
        CharacterData character = data.CharacterIndex < CharacterDatabase.Characters.Length
            ? CharacterDatabase.Characters[data.CharacterIndex]
            : CharacterDatabase.Characters[0];
        if (character == null)
        {
            GD.PrintErr($"[SplitScreenManager] Character not found for index {data.CharacterIndex}, using fallback");
            character = CharacterDatabase.Characters[0];
        }

        Player player = PlayerScene.Instantiate<Player>();
        LevelNode.AddChild(player);
        bool isLocalOwner = data.SteamId == Network.Lobby.LocalSteamId;

        if (isLocalOwner)
        {
            LobbyPlayerData lobbyData = GameManager.Instance.LobbyPlayers
                .Find(p => p.PlayerId == data.PlayerId);

            int deviceId = lobbyData?.DeviceId ?? -1;
            string deviceType = lobbyData?.DeviceType ?? "Keyboard";

            InitiateOnlinePlayer(player, character, data, deviceId, deviceType);
            CreateLocalViewport(player);
        }
        else
            InitiateOnlinePlayer(player, character, data);
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

    private void CreateNetworkServices()
    {
        _pickupService = new PickupNetworkService();
        AddChild(_pickupService);
        _pickupService.Initialize(_levelNode);

        _farmService = new FarmNetworkService();
        AddChild(_farmService);
        _farmService.Initialize();

        _toolService = new ToolNetworkService();
        AddChild(_toolService);
    }

    private void FinalizeMatchSetup()
    {
        UpdateViewportLayout();
        GameManager.Instance.StartMatch(_levelNode);

        if (Network.IsOnline)
        {
            _matchManager.SubscribeToTimerSync();
            _toolService.Initialize(_matchManager.GetPlayers());
        }

        CreateMatchResultsPanel();
    }


    private void InitiateOnlinePlayer(Player player, CharacterData character, PlayerSpawnData data, int deviceId=-1, string deviceType="Remote")
    {
        player.Setup(data.PlayerId, character, deviceId, deviceType);
        player.SetNetworkOwnership(data.SteamId, Network.Lobby.LocalSteamId);
        _matchManager.RegisterPlayer(player, data.SpawnIndex);
        _pickupService.RegisterPlayer(player);
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