using Godot;
using System;

public partial class UIPlayerViewport : SubViewportContainer
{
    [Export] public PackedScene PlayerCamScene { get; set; }
    [Export] public PackedScene UIPlayerHotbarScene { get; set; }
    [Export] public PackedScene UIPlayerWaterScene { get; set; }
    [Export] public PackedScene UIPlayerTimerScene { get; set; }

    [ExportGroup("Debug Camera")]
    [Export] public bool DebugWorldCam = false;
    [Export] public float DebugCameraScale = 1f;

    private SubViewport SubPort;
    private CanvasLayer Layer;

    private Camera2D Cam;
    private UIPlayerHotbar UIHotbar;
    private UIPlayerWater UIWater;
    private UIPlayerTimer UIPlayerTimer;

    public override void _Ready()
	{
		SubPort = GetNode<SubViewport>("SubViewport");
        Layer = SubPort.GetNode<CanvasLayer>("CanvasLayer");

        ConfigureViewport();
        SetPackedScenes();
	}

    public SubViewport GetSubViewport()
    {
        return SubPort;
    }

    public void SetSubPortWorld(World2D world)
    {
        if (SubPort == null || world == null)
            return;

        SubPort.World2D = world;
    }

    public void SetCameraBounds(Rect2I rect, Vector2I tileSize)
    {
        if (Cam == null)
            return;

        Vector2 worldPos = rect.Position * tileSize;
        Vector2 worldSize = rect.Size * tileSize;

        Cam.LimitLeft = (int)worldPos.X;
        Cam.LimitTop = (int)worldPos.Y;
        Cam.LimitRight = (int)(worldPos.X + worldSize.X);
        Cam.LimitBottom = (int)(worldPos.Y + worldSize.Y);
    }

    public void LinkPlayer(Player newPlayerNode)
    {
        if (newPlayerNode == null || Cam == null)
            return;

		var remote = newPlayerNode.GetNode<RemoteTransform2D>("RemoteTransform2D");
		remote.RemotePath = Cam.GetPath();
    }

    public void LinkPlayerUI(Player newPlayerNode)
    {
        if (newPlayerNode == null)
            return;

        if (UIHotbar != null)
        {
            var hotbar = newPlayerNode.GetNode<PlayerHotbar>("PlayerHotbar");
            UIHotbar.Bind(hotbar);
        }

        if (UIWater != null)
        {
            var water = newPlayerNode.GetNode<PlayerWater>("PlayerWater");
            UIWater.Bind(water);
        }
    }

    public void UpdateViewportSize(Vector2I size, float baseHeight, int gridSpaces)
    {
        if (SubPort == null)
            return;

        SubPort.Size = size;
        float scale = SubPort.Size.Y / baseHeight;

        if (Cam != null)
            Cam.Zoom = new Vector2(scale, scale);
        
        if (DebugWorldCam)
            Cam.Zoom = new Vector2(DebugCameraScale, DebugCameraScale);
        
        ScaleUI(1f / gridSpaces);
    }

    private void SetPackedScenes()
    {
        if (PlayerCamScene != null)
		{
			Cam = PlayerCamScene.Instantiate<Camera2D>();
            Cam.Name = "PlayerCamera";
            Cam.PositionSmoothingEnabled = false;
			SubPort.AddChild(Cam);
		}

		if (UIPlayerHotbarScene != null)
		{
			UIHotbar = UIPlayerHotbarScene.Instantiate<UIPlayerHotbar>();
			Layer.AddChild(UIHotbar);
		}

		if (UIPlayerWaterScene != null)
		{
			UIWater = UIPlayerWaterScene.Instantiate<UIPlayerWater>();
			Layer.AddChild(UIWater);
		}
        if (UIPlayerTimerScene != null)
        {
            UIPlayerTimer = UIPlayerTimerScene.Instantiate<UIPlayerTimer>();
            Layer.AddChild(UIPlayerTimer);
        }
    }

    private void ConfigureViewport()
    {
        TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
        SubPort.CanvasItemDefaultTextureFilter = Viewport.DefaultCanvasItemTextureFilter.Nearest;
        SubPort.Disable3D = true;
    }

    private void ScaleUI(float scale)
    {
        foreach (Node child in Layer.GetChildren())
        {
            if (child is Control control)
                control.Scale = new Vector2(scale, scale);
        }
    }
}