using Godot;
using System;

public partial class SplitScreenManager : Node
{
    public static SplitScreenManager Instance;

    [Export] public PackedScene PlayerCamScene { get; set; }
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

        SetupViewport(container, subPort, cam);

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
            {
                var levelScene = GD.Load<PackedScene>(LevelPath);
                levelNode = levelScene.Instantiate<Node>();

                subPort.AddChild(levelNode);
                firstSubViewport = subPort;
            }
        }
		
        LinkPlayerCam(newPlayerNode, cam);
        UpdateViewportSize();
    }

    private void SetupViewport(SubViewportContainer container, SubViewport subPort, Camera2D cam)
    {
        container.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
        subPort.CanvasItemDefaultTextureFilter = Viewport.DefaultCanvasItemTextureFilter.Nearest;
        subPort.Disable3D = true;
        cam.PositionSmoothingEnabled = false;
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