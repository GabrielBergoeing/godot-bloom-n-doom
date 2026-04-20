using Godot;
using System;

public partial class SplitScreenManager : Node
{
    public static SplitScreenManager Instance;

    [Export] public PackedScene PlayerCamScene { get; set; }
    [Export] public PackedScene UIPlayerHotbarScene { get; set; }
    [Export] public string LevelPath { get; set; }

    private GridContainer screenContainer;

    private SubViewport firstSubViewport = null;
    private Node levelNode = null;
	public Node LevelNode => levelNode;

	private bool firstPlayerAssigned = false;

	public override void _Ready()
	{
		Instance = this;
		screenContainer = GetNode<GridContainer>("CenterContainer/GridContainer");
		AddNewPlayerViewport(null);
	}

    public void AddNewPlayerViewport(Player newPlayerNode)
    {
        var container = new SubViewportContainer();
        var subPort = new SubViewport();
        var cam = PlayerCamScene.Instantiate<Camera2D>();

        ConfigureViewport(container, subPort, cam);

		if (!firstPlayerAssigned && firstSubViewport != null)
		{
			subPort = firstSubViewport;
			firstPlayerAssigned = true;
            cam = subPort.GetNode<Camera2D>("PlayerCamera");
		}
        else
        {
            screenContainer.AddChild(container);
            container.AddChild(subPort);
            subPort.AddChild(cam);

            if (firstSubViewport != null)
                subPort.World2D = firstSubViewport.World2D;
            else
                FirstViewportSetup(subPort, cam);
        }

        SetCameraBounds(cam);
        LinkPlayerCam(newPlayerNode, cam);
        LinkPlayerUI(newPlayerNode, subPort);
        UpdateViewportSize();
    }

    private void ConfigureViewport(SubViewportContainer container, SubViewport subPort, Camera2D cam)
    {
        container.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
        subPort.CanvasItemDefaultTextureFilter = Viewport.DefaultCanvasItemTextureFilter.Nearest;
        subPort.Disable3D = true;
        cam.PositionSmoothingEnabled = false;
    }

    private void FirstViewportSetup(SubViewport subPort, Camera2D cam)
    {
        var levelScene = GD.Load<PackedScene>(LevelPath);
        levelNode = levelScene.Instantiate<Node>();

        subPort.AddChild(levelNode);
        firstSubViewport = subPort;
    }

    private void SetCameraBounds(Camera2D cam)
    {
        var tilemap = levelNode.GetNode<TileMapLayer>("TileMapLayer");
        var rect = tilemap.GetUsedRect();
        var tileSize = tilemap.TileSet.TileSize;

        Vector2 worldPos = rect.Position * tileSize;
        Vector2 worldSize = rect.Size * tileSize;

        cam.LimitLeft = (int)worldPos.X;
        cam.LimitTop = (int)worldPos.Y;
        cam.LimitRight = (int)(worldPos.X + worldSize.X);
        cam.LimitBottom = (int)(worldPos.Y + worldSize.Y);
    }

    private void LinkPlayerCam(Player newPlayerNode, Camera2D cam)
    {
        if (newPlayerNode != null && cam != null)
		{
			var remote = newPlayerNode.GetNode<RemoteTransform2D>("RemoteTransform2D");
			remote.RemotePath = cam.GetPath();
		}
    }

    private void UpdateViewportSize()
    {
        int count = screenContainer.GetChildCount();

        if (count == 0)
            return;

        int columns = Mathf.CeilToInt(count / 2.0f);
        screenContainer.Columns = columns;

        foreach (Node child in screenContainer.GetChildren())
        {
            var container = child as SubViewportContainer;

            if (container == null || container.GetChildCount() == 0)
                continue;

            var subPort = container.GetChild<SubViewport>(0);
            Vector2 size = GetViewport().GetVisibleRect().Size;

            subPort.Size = SetSubPortSize(count, columns, size);
        }
    }

    private void LinkPlayerUI(Player newPlayerNode, SubViewport subPort)
    {
        if (UIPlayerHotbarScene == null || newPlayerNode == null)
            return;

        var canvasLayer = new CanvasLayer();
        canvasLayer.Layer = 10;
        subPort.AddChild(canvasLayer);

        var ui = UIPlayerHotbarScene.Instantiate<UIPlayerHotbar>();
        canvasLayer.AddChild(ui);

        var hotbar = newPlayerNode.GetNode<PlayerHotbar>("PlayerHotbar");
        ui.Bind(hotbar);
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