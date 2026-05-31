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
        GD.Print("[SplitScreenManager] PrepareOnlineMatch");
        SteamMatchManager.Instance.OnMatchStarted += OnMatchStarted;

        // Tell SteamMatchManager the scene is ready to receive the packet
        SteamMatchManager.Instance.NotifyMatchSceneReady();

        if (Network.Match.IsHost)
        {
            GD.Print("[SplitScreenManager] Host broadcasting match start");
            SteamMatchManager.Instance.BroadcastMatchStart();
        }
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
        GD.Print("[SplitScreenManager] Creating level");
        UIPlayerViewport viewport = CreateViewport();

        LevelData levelData = GameManager.Instance.CurrentLevel;
        _levelNode = levelData.LevelScene.Instantiate<Node>();

        viewport.GetSubViewport().AddChild(_levelNode);

        _sharedWorld = viewport.GetSubViewport().World2D;
        _matchManager =_levelNode.GetNode<MatchManager>("MatchManager");
        GD.Print("[SplitScreenManager] Level created successfully");
    }

    private void SpawnOfflinePlayers()
    {
        var players = GameManager.Instance.LobbyPlayers;
        for (int i = 0; i < players.Count; i++)
            SpawnOfflinePlayer(players[i], i);
    }

    private void SpawnOnlinePlayers(MatchStartPacket packet)
    {
        GD.Print($"[SplitScreenManager] Spawning {packet.Players.Count} online players");
        foreach (var spawnData in packet.Players)
            SpawnNetworkPlayer(spawnData);
    }

    private void SpawnOfflinePlayer(LobbyPlayerData data, int spawnIndex)
    {
        CharacterData character = data.SelectedCharacter
            ?? CharacterDatabase.Characters[0];

        Player player = PlayerScene.Instantiate<Player>();
        LevelNode.AddChild(player);

        player.Setup(data.PlayerId, data.DeviceId, data.DeviceType, character.Sprites);
        _matchManager.RegisterPlayer(player, spawnIndex);
        CreateLocalViewport(player);
    }

    private void SpawnNetworkPlayer(PlayerSpawnData data)
    {
        GD.Print($"[SplitScreenManager] SpawnNetworkPlayer -> PlayerId: {data.PlayerId}, SteamId: {data.SteamId}, Local: {data.IsLocalOwner}, Char: {data.CharacterIndex}, Spawn: {data.SpawnIndex}");

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

        if (data.IsLocalOwner)
        {
            GD.Print($"[SplitScreenManager] Configuring LOCAL player {data.PlayerId}");

            LobbyPlayerData lobbyData = GameManager.Instance.LobbyPlayers
                .Find(p => p.PlayerId == data.PlayerId);

            int deviceId = lobbyData?.DeviceId ?? -1;
            string deviceType = lobbyData?.DeviceType ?? "Keyboard";

            player.Setup(data.PlayerId, deviceId, deviceType, character.Sprites);
            player.SetNetworkOwnership(data.SteamId, Network.Lobby.LocalSteamId);
            _matchManager.RegisterPlayer(player, data.SpawnIndex);
            CreateLocalViewport(player);
        }
        else
        {
            GD.Print($"[SplitScreenManager] Configuring REMOTE player {data.PlayerId}");
            player.Setup(data.PlayerId, -1, "Remote", character.Sprites);
            player.SetNetworkOwnership(data.SteamId, Network.Lobby.LocalSteamId);
            _matchManager.RegisterPlayer(player, data.SpawnIndex);
        }
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
        GD.Print($"[SplitScreenManager] Viewport linked to Player {player.PlayerId}");
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
        GD.Print("[SplitScreenManager] FinalizeMatchSetup");
        UpdateViewportLayout();
        GameManager.Instance.StartMatch(_levelNode);

        if (Network.IsOnline)
        {
            _matchManager.SubscribeToTimerSync();
            GD.Print("[SplitScreenManager] Calling PickupNetworkService.Initialize");
            PickupNetworkService.Instance.Initialize(_levelNode);
        }

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