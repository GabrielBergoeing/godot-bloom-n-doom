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

    private SubViewport _subPort;
    private CanvasLayer _layer;

    private Camera2D _cam;
    private UIPlayerHotbar _uiHotbar;
    private UIPlayerWater _uiWater;
    private UIPlayerTimer _uiPlayerTimer;

    public override void _Ready()
	{
		_subPort = GetNode<SubViewport>("SubViewport");
        _layer = _subPort.GetNode<CanvasLayer>("CanvasLayer");

        ConfigureViewport();
        SetPackedScenes();
	}

    public SubViewport GetSubViewport()
    {
        return _subPort;
    }

    public void SetSubPortWorld(World2D world)
    {
        if (_subPort == null || world == null)
            return;

        _subPort.World2D = world;
    }

    public void SetCameraBounds(Rect2I rect, Vector2I tileSize)
    {
        if (_cam == null)
            return;

        Vector2 worldPos = rect.Position * tileSize;
        Vector2 worldSize = rect.Size * tileSize;

        _cam.LimitLeft = (int)worldPos.X;
        _cam.LimitTop = (int)worldPos.Y;
        _cam.LimitRight = (int)(worldPos.X + worldSize.X);
        _cam.LimitBottom = (int)(worldPos.Y + worldSize.Y);
    }

    public void LinkPlayer(Player newPlayerNode)
    {
        if (newPlayerNode == null || _cam == null)
            return;

		var remote = newPlayerNode.GetNode<RemoteTransform2D>("RemoteTransform2D");
		remote.RemotePath = _cam.GetPath();
    }

    public void LinkPlayerUI(Player newPlayerNode)
    {
        if (newPlayerNode == null)
            return;

        if (_uiHotbar != null)
        {
            var hotbar = newPlayerNode.GetNode<PlayerHotbar>("PlayerHotbar");
            _uiHotbar.Bind(hotbar);
        }

        if (_uiWater != null)
        {
            var water = newPlayerNode.GetNode<PlayerWater>("PlayerWater");
            _uiWater.Bind(water);
        }
    }

    public void UpdateViewportSize(Vector2I size, float baseHeight, int gridSpaces)
    {
        if (_subPort == null)
            return;

        _subPort.Size = size;
        float scale = _subPort.Size.Y / baseHeight;

        if (_cam != null)
            _cam.Zoom = new Vector2(scale, scale);
        
        if (DebugWorldCam)
            _cam.Zoom = new Vector2(DebugCameraScale, DebugCameraScale);
        
        ScaleUI(1f / gridSpaces);
    }

    private void SetPackedScenes()
    {
        if (PlayerCamScene != null)
		{
			_cam = PlayerCamScene.Instantiate<Camera2D>();
            _cam.Name = "PlayerCamera";
            _cam.PositionSmoothingEnabled = false;
			_subPort.AddChild(_cam);
		}

		if (UIPlayerHotbarScene != null)
		{
			_uiHotbar = UIPlayerHotbarScene.Instantiate<UIPlayerHotbar>();
			_layer.AddChild(_uiHotbar);
		}

		if (UIPlayerWaterScene != null)
		{
			_uiWater = UIPlayerWaterScene.Instantiate<UIPlayerWater>();
			_layer.AddChild(_uiWater);
		}
        if (UIPlayerTimerScene != null)
        {
            _uiPlayerTimer = UIPlayerTimerScene.Instantiate<UIPlayerTimer>();
            _layer.AddChild(_uiPlayerTimer);
        }
    }

    private void ConfigureViewport()
    {
        TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
        _subPort.CanvasItemDefaultTextureFilter = Viewport.DefaultCanvasItemTextureFilter.Nearest;
        _subPort.Disable3D = true;
    }

    private void ScaleUI(float scale)
    {
        foreach (Node child in _layer.GetChildren())
        {
            if (child is Control control)
                control.Scale = new Vector2(scale, scale);
        }
    }
}