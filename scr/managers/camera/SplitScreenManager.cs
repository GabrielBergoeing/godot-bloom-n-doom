using Godot;
using System.Collections.Generic;

public partial class SplitScreenManager : Node
{
    public static SplitScreenManager Instance;

    [Export] public PackedScene UIPlayerViewportScene;
    [Export] public PackedScene PlayerScene;
    [Export] public PackedScene MatchResultsScene;

    private GridContainer _screenContainer;
    private Node _levelNode;
    public Node LevelNode => _levelNode;
    private UIMatchResults _matchResults;

    private World2D _sharedWorld;
    private readonly List<UIPlayerViewport> _viewports = new();

    private const float BaseHeight = 162f;

    public override void _Ready()
    {
        Instance = this;
        _screenContainer = GetNode<GridContainer>("CenterContainer/GridContainer");

        CreateLevel();
        SpawnPlayers();
        UpdateViewportLayout();
        GameManager.Instance.StartMatch(_levelNode);
        CreateMatchResultsPanel();
    }

    private void CreateLevel()
    {
        LevelData levelData = GameManager.Instance.CurrentLevel;
        UIPlayerViewport worldViewport = CreateViewport();

        _levelNode =levelData.LevelScene.Instantiate<Node>();
        worldViewport.GetSubViewport().AddChild(_levelNode);
        _sharedWorld = worldViewport.GetSubViewport().World2D;
    }

    private void SpawnPlayers()
    {
        var players = GameManager.Instance.LobbyPlayers;
        for (int i = 0; i < players.Count; i++)
            SpawnPlayer(players[i], i);
    }

    private void SpawnPlayer(LobbyPlayerData data, int index)
    {
        Player player = PlayerScene.Instantiate<Player>();
        LevelNode.AddChild(player);

        player.Setup(
            data.PlayerId,
            data.DeviceId,
            data.DeviceType,
            data.SelectedCharacter.Sprites
        );

        UIPlayerViewport viewport;
        if (index == 0)
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
        if (_levelNode == null)
            return;

        var tilemap =_levelNode.GetNode<TileMapLayer>("World");

        viewport.SetCameraBounds(
            tilemap.GetUsedRect(),
            tilemap.TileSet.TileSize
        );
    }

    private void UpdateViewportLayout()
    {
        int count = _viewports.Count;
        if (count == 0)
            return;

        int columns = Mathf.CeilToInt(count / 2.0f);
        _screenContainer.Columns = columns;

        Vector2 size = GetViewport().GetVisibleRect().Size;

        Vector2I viewportSize =
            CalculateViewportSize(
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

    private Vector2I CalculateViewportSize(int count, int columns, Vector2 screenSize)
    {
        int rows = Mathf.CeilToInt(count / (float)columns);
        int width = (int)(screenSize.X / columns);
        int height = (int)(screenSize.Y / rows);

        width -= width % 2;
        height -= height % 2;

        return new Vector2I(width, height);
    }

    private void CreateMatchResultsPanel()
    {
        _matchResults = MatchResultsScene.Instantiate<UIMatchResults>();
        AddChild(_matchResults);
    }
}