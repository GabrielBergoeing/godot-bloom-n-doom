using Godot;
using System.Collections.Generic;

public partial class Player : Entity
{
	[Export] private PackedScene HotbarScene;
	[Export] private PackedScene TileInteractionScene;
	[Export] private PackedScene WaterScene;
	[Export] private PackedScene WaterFXScene;

	public PlayerInput Input { get; private set; }
	public PlayerHotbar Hotbar { get; private set; }
	public PlayerTileInteraction Tile { get; private set; }
	public PlayerWater Water { get; private set; }
	public PlayerWaterFX WaterFX { get; private set; }

	public int PlayerId { get; private set; }
	public List<Pickup> PickupsInRange = new();

	// States
	public PlayerIdleState IdleState { get; private set; }
	public PlayerMoveState MoveState { get; private set; }
	public PlayerUseItemState UseItemState { get; private set; }
	public PlayerRemoveState RemoveState { get; private set; }
	public PlayerIrrigateState IrrigateState { get; private set; }
	public PlayerPrepareGroundState PrepareGroundState { get; private set; }
	public PlayerShootState ShootState { get; private set; }

	public override void _Ready()
	{
		base._Ready();
		AddToGroup("players");

		GetPlayerSystems();
		PreparePlayerState();
		stateMachine.Initialize(IdleState);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		Hotbar.HandleInput(Input);
	}

	public override void _PhysicsProcess(double delta)
	{
		MoveAndSlide();
	}

	public void AssignDevice(int deviceId, string deviceType, int playerId)
	{
		PlayerId = playerId;
		Input.AssignDevice(deviceId, deviceType, playerId);
	}

	public Vector2 GetFacingDirection() =>
		Input.FacingDir;

	public Pickup GetPickupNearby()
	{
		Pickup closest = null;
		float minDist = float.MaxValue;

		foreach (var p in PickupsInRange)
		{
			float dist = GlobalPosition.DistanceTo(p.GlobalPosition);
			if (dist < minDist)
			{
				minDist = dist;
				closest = p;
			}
		}

		return closest;
	}

	public void SpawnPickup(ItemData data)
	{
		if (data?.PickupScene == null)
			return;

		var pickup = data.PickupScene.Instantiate<Pickup>();
		if (pickup == null)
		{
			GD.PrintErr("PickupScene is not a Pickup!");
			return;
		}

		var level = SplitScreenManager.Instance?.LevelNode;
		if (level == null)
		{
			GD.PrintErr("LevelNode not found!");
			return;
		}

		level.AddChild(pickup);
		pickup.GlobalPosition = GlobalPosition;
		pickup.SetItemData(data);
	}

	private void GetPlayerSystems() 
	{
		Input = GetNode<PlayerInput>("PlayerInput");

		if (HotbarScene != null)
		{
			Hotbar = HotbarScene.Instantiate<PlayerHotbar>();
			AddChild(Hotbar);
		}

		if (TileInteractionScene != null)
		{
			Tile = TileInteractionScene.Instantiate<PlayerTileInteraction>();
			AddChild(Tile);
		}

		if (WaterScene != null)
		{
			Water = WaterScene.Instantiate<PlayerWater>();
			AddChild(Water);
		}

		if (WaterFXScene != null)
		{
			WaterFX = WaterFXScene.Instantiate<PlayerWaterFX>();
			AddChild(WaterFX);
		}

		var onHand = GetNode<PlayerOnHand>("PlayerOnHand");
    	onHand?.Initialize(this, Hotbar);
	}

	private void PreparePlayerState()
	{
		IdleState = new PlayerIdleState(this, stateMachine);
		MoveState = new PlayerMoveState(this, stateMachine);
		UseItemState = new PlayerUseItemState(this, stateMachine);
		RemoveState = new PlayerRemoveState(this, stateMachine);
		IrrigateState = new PlayerIrrigateState(this, stateMachine);
		PrepareGroundState = new PlayerPrepareGroundState(this, stateMachine);
		ShootState = new PlayerShootState(this, stateMachine);
	}
}
