using Godot;
using System;

public partial class SplitScreenManager : Node
{
    public static SplitScreenManager Instance;

    [Export] public PackedScene UIPlayerViewportScene { get; set; }
    [Export] public int MaxPlayerCount = 4;

    private LevelData LevelPath { get; set; }
    private GridContainer screenContainer;
    private SubViewport firstSubViewport = null;
    private UIPlayerViewport bootstrapViewport;

    private Node levelNode = null;
	public Node LevelNode => levelNode;

    private float baseHeight = 162;

    public override void _Ready()
    {
        Instance = this;
        LevelPath = SceneManager.Instance.CurrentLevel;

        screenContainer = GetNode<GridContainer>("CenterContainer/GridContainer");
        bootstrapViewport = UIPlayerViewportScene.Instantiate<UIPlayerViewport>();

        AddChild(bootstrapViewport);
        FirstViewportSetup(bootstrapViewport);

        firstSubViewport = bootstrapViewport.GetSubViewport();
    }

    public void AddNewPlayerViewport(Player newPlayerNode)
    {
        var container = UIPlayerViewportScene.Instantiate<UIPlayerViewport>();

        screenContainer.AddChild(container);
        container.SetSubPortWorld(firstSubViewport.World2D);

        if (levelNode != null)
        {
            var tilemap = levelNode.GetNode<TileMapLayer>("World");

            container.SetCameraBounds(
                tilemap.GetUsedRect(),
                tilemap.TileSet.TileSize
            );
        }

        container.LinkPlayer(newPlayerNode);
        container.LinkPlayerUI(newPlayerNode);

        UpdateViewportSize();
        RemoveBootstrapViewport();
    }

    private void FirstViewportSetup(UIPlayerViewport container)
    {
        var levelScene = LevelPath.LevelScene;
        levelNode = levelScene.Instantiate<Node>();
        container.GetSubViewport().AddChild(levelNode);
    }

    private void UpdateViewportSize()
    {
        int count = screenContainer.GetChildCount();
        if (count == 0)
            return;

        int columns = Mathf.CeilToInt(count / 2.0f);
        screenContainer.Columns = columns;

        Vector2 size = GetViewport().GetVisibleRect().Size;
        foreach (Node child in screenContainer.GetChildren())
        {
            var container = child as UIPlayerViewport;

            if (container == null)
                continue;

            Vector2I subPortSize = SetSubPortSize(count, columns, size);
            container.UpdateViewportSize(subPortSize, baseHeight, count);
        }
    }

    private void RemoveBootstrapViewport()
    {
        if (bootstrapViewport == null)
            return;

        if (screenContainer.GetChildCount() > 0)
        {
            bootstrapViewport.Visible = false;
            var subPort = bootstrapViewport.GetSubViewport();

            if (subPort != null)
            {
                subPort.RenderTargetUpdateMode =
                    SubViewport.UpdateMode.Disabled;
            }
        }
    }

    private Vector2I SetSubPortSize(int count, int columns, Vector2 size)
    {
        int rows = Mathf.CeilToInt(count / (float)columns);

        int width = (int)(size.X / columns);
        int height = (int)(size.Y / rows);

        width -= width % 2;
        height -= height % 2;
        return new Vector2I(width, height);
    }
}